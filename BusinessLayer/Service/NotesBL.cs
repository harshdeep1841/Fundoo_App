using System.Text.Json;
using System.Text.Json.Serialization;
using BusinessLayer.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Models;
using Org.BouncyCastle.Tls.Crypto;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace BusinessLayer.Service;

public class NotesBL : INotesBL
{
    public INotesRL notesRL;
    public IDistributedCache cache;
    public ILogger<NotesBL> logger;

    public NotesBL(INotesRL notes , IDistributedCache cache ,  ILogger<NotesBL> logger)
    {
        notesRL = notes;
        this.cache = cache;
        this.logger = logger;
    }
    public async Task<ResponseModel<NoteEntity>> AddNotes(
        NoteCreationModel model,
        int userId)
    {
        var note = new NoteEntity
        {
            Title = model.Title,
            Description = model.Description,
            Color = model.Colour,
            UserId = userId
        };

        var createdNote = await notesRL.AddNotes(note);

        await InvalidateNotesCache(userId);

        return new ResponseModel<NoteEntity>(
            true,
            "Note created successfully.",
            createdNote
        );
    }
    
    public async Task<ResponseModel<List<NoteEntity>>> ViewNotes(int userId)
    {
        string cacheKey = $"User:{userId}:Notes";

        try
        {
            string? jsonString = await cache.GetStringAsync(cacheKey);

            if (jsonString != null)
            {
                var notesFromCache =
                    JsonSerializer.Deserialize<List<NoteEntity>>(jsonString);

                if (notesFromCache != null)
                {
                    return new ResponseModel<List<NoteEntity>>(
                        true,
                        notesFromCache.Count == 0
                            ? "No notes found."
                            : "Notes retrieved successfully (from cache).",
                        notesFromCache
                    );
                }
            }
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Redis read failed.Fetching notes from PostgreSQL for user {UserId}.",
                userId);
        }

        var notes = await notesRL.ViewNotes(userId);

        try
        {
            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var serializedNotes = JsonSerializer.Serialize(notes);

            await cache.SetStringAsync(
                cacheKey,
                serializedNotes,
                cacheEntryOptions);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Redis write failed.Returning notes from PostgreSQL for user {UserId}.",
                userId);
        }

        return new ResponseModel<List<NoteEntity>>(
            true,
            notes.Count == 0
                ? "No notes found."
                : "Notes retrieved successfully.",
            notes
        );
    }    
    public async Task<ResponseModel<NoteEntity>> ViewNotesById(
        int id, int userId)
    {
        var note = await notesRL.ViewNotesById(id, userId);

        if (note == null)
        {
            return new ResponseModel<NoteEntity>(
                false, "Note not found.", null!);
        }

        return new ResponseModel<NoteEntity>(
            true, "Note retrieved successfully.", note);
    }

    public async Task<ResponseModel<NoteEntity>> EditNote(
        int id, EditNoteModel model, int userId)
    {
        var updatedNote = new NoteEntity
        {
            Id = id,
            Title = model.Title,
            Description = model.Description,
            Color = model.Colour
        };

        var note = await notesRL.EditNote(updatedNote, userId);

        if (note == null)
        {
            return new ResponseModel<NoteEntity>(
                false, "Note not found.", null!);
        }
        await InvalidateNotesCache(userId);

        return new ResponseModel<NoteEntity>(
            true, "Note updated successfully.", note);
    }

    public async Task<ResponseModel<bool>> DeleteNote(
        int id, int userId)
    {
        var deleted = await notesRL.DeleteNote(id, userId);

        if (!deleted)
        {
            return new ResponseModel<bool>(
                false, "Note not found.", false);
        }
        await InvalidateNotesCache(userId);

        return new ResponseModel<bool>(
            true, "Note moved to trash successfully.", true);
    }

    public async Task<ResponseModel<bool>> ToggleArchive(
        int id, int userId)
    {
        var updated = await notesRL.ToggleArchive(id, userId);

        if (!updated)
        {
            return new ResponseModel<bool>(
                false, "Note not found.", false);
        }

        await InvalidateNotesCache(userId);
        return new ResponseModel<bool>(
            true, "Archive status updated successfully.", true);
    }

    public async Task<ResponseModel<bool>> ToggleTrash(
        int id, int userId)
    {
        var updated = await notesRL.ToggleTrash(id, userId);

        if (!updated)
        {
            return new ResponseModel<bool>(
                false, "Note not found.", false);
        }
        await InvalidateNotesCache(userId);

        return new ResponseModel<bool>(
            true, "Trash status updated successfully.", true);
    }
    
    
    //helper
    private async Task InvalidateNotesCache(int userId)
    {
        string cacheKey = $"User:{userId}:Notes";

        await cache.RemoveAsync(cacheKey);
    }
}