using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.DTOs;
using FullStackAPI_Guild.Api.Entities;
using FullStackAPI_Guild.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace FullStackAPI_Guild.Api.Services;

public class CharacterService
{
    private static string GetDefaultImageUrl(CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.DarkWizard => "/images/characters/defaults/dark-wizard-default.jpg",
            CharacterClass.DarkKnight => "/images/characters/defaults/dark-knight-default.jpg",
            CharacterClass.FairyElf => "/images/characters/defaults/fairy-elf-default.jpg",
            CharacterClass.MagicGladiator => "/images/characters/defaults/magic-gladiator-default.jpg",
            CharacterClass.DarkLord => "/images/characters/defaults/dark-lord-default.jpg",
            _ => "/images/characters/defaults/default.jpg"
        };
    }

    private readonly AppDbContext _context;

    public CharacterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CharacterResponse> CreateAsync(Guid userId, CreateCharacterRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Usuario invalido.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Nome do personagem e obrigatorio.");

        if (!Enum.TryParse<CharacterClass>(request.CharacterClass, true, out var characterClass))
            throw new InvalidOperationException("Classe invalida.");

        if (!Enum.TryParse<CharacterRoleTag>(request.RoleTag, true, out var roleTag))
            throw new InvalidOperationException("Tag invalida.");

        if (request.PrioritySlot < 1 || request.PrioritySlot > 5)
            throw new InvalidOperationException("PrioritySlot deve estar entre 1 e 5.");

        if (request.LastKnownLevel < 0 || request.LastKnownLevel > 400)
            throw new InvalidOperationException("LastKnownLevel deve estar entre 0 e 400.");

        var activeCount = await _context.Characters.CountAsync(x => x.UserId == userId && x.IsActive);
        if (activeCount >= 5)
            throw new InvalidOperationException("O usuario ja possui 5 personagens ativos.");

        var slotInUse = await _context.Characters.AnyAsync(x =>
            x.UserId == userId &&
            x.IsActive &&
            x.PrioritySlot == request.PrioritySlot);

        if (slotInUse)
            throw new InvalidOperationException("Ja existe personagem ativo nesse PrioritySlot.");

        var defaultImageUrl = GetDefaultImageUrl(characterClass);

        var character = new Character
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            CharacterClass = characterClass,
            RoleTag = roleTag,
            PrioritySlot = request.PrioritySlot,
            LastKnownLevel = request.LastKnownLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Characters.Add(character);
        await _context.SaveChangesAsync();

        var image = new CharacterImage
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            ImageUrl = defaultImageUrl,
            FileName = Path.GetFileName(defaultImageUrl),
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.CharacterImages.Add(image);
        await _context.SaveChangesAsync();

        character.CurrentImageId = image.Id;
        character.CurrentImage = image;

        await _context.SaveChangesAsync();

        return Map(character);
    }

    public async Task<CharacterResponse> UpdateAsync(Guid userId, Guid characterId, UpdateCharacterRequest request)
    {
        var character = await _context.Characters
            .Include(x => x.CurrentImage)
            .FirstOrDefaultAsync(x => x.Id == characterId && x.UserId == userId);

        if (character is null)
            throw new InvalidOperationException("Personagem nao encontrado.");

        if (!Enum.TryParse<CharacterRoleTag>(request.RoleTag, true, out var roleTag))
            throw new InvalidOperationException("Tag invalida.");

        if (request.PrioritySlot < 1 || request.PrioritySlot > 5)
            throw new InvalidOperationException("PrioritySlot deve estar entre 1 e 5.");

        if (request.LastKnownLevel < 0 || request.LastKnownLevel > 400)
            throw new InvalidOperationException("LastKnownLevel deve estar entre 0 e 400.");

        if (character.IsActive)
        {
            var slotInUse = await _context.Characters.AnyAsync(x =>
                x.UserId == userId &&
                x.IsActive &&
                x.PrioritySlot == request.PrioritySlot &&
                x.Id != character.Id);

            if (slotInUse)
                throw new InvalidOperationException("Ja existe personagem ativo nesse PrioritySlot.");
        }

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            var normalizedImageUrl = request.ImageUrl.Trim();

            if (!Uri.IsWellFormedUriString(normalizedImageUrl, UriKind.RelativeOrAbsolute))
                throw new InvalidOperationException("ImageUrl invalida.");

            var newImage = new CharacterImage
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                ImageUrl = normalizedImageUrl,
                FileName = Path.GetFileName(normalizedImageUrl),
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.CharacterImages.Add(newImage);

            character.CurrentImageId = newImage.Id;
            character.CurrentImage = newImage;
        }

        character.RoleTag = roleTag;
        character.PrioritySlot = request.PrioritySlot;
        character.LastKnownLevel = request.LastKnownLevel;
        character.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(character);
    }

    public async Task<CharacterResponse> DeactivateAsync(Guid userId, Guid characterId)
    {
        var character = await _context.Characters
            .Include(x => x.CurrentImage)
            .FirstOrDefaultAsync(x => x.Id == characterId && x.UserId == userId);

        if (character is null)
            throw new InvalidOperationException("Personagem nao encontrado.");

        if (!character.IsActive)
            throw new InvalidOperationException("Personagem ja esta inativo.");

        character.IsActive = false;
        character.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(character);
    }

    public async Task<CharacterResponse> ReactivateAsync(Guid userId, Guid characterId)
    {
        var character = await _context.Characters
            .Include(x => x.CurrentImage)
            .FirstOrDefaultAsync(x => x.Id == characterId && x.UserId == userId);

        if (character is null)
            throw new InvalidOperationException("Personagem nao encontrado.");

        if (character.IsActive)
            throw new InvalidOperationException("Personagem ja esta ativo.");

        var activeCount = await _context.Characters.CountAsync(x => x.UserId == userId && x.IsActive);
        if (activeCount >= 5)
            throw new InvalidOperationException("O usuario ja possui 5 personagens ativos.");

        var slotInUse = await _context.Characters.AnyAsync(x =>
            x.UserId == userId &&
            x.IsActive &&
            x.PrioritySlot == character.PrioritySlot &&
            x.Id != character.Id);

        if (slotInUse)
            throw new InvalidOperationException("Ja existe personagem ativo nesse PrioritySlot.");

        character.IsActive = true;
        character.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(character);
    }

    public async Task<List<GuildCharacterBoardItemResponse>> GetGuildBoardAsync()
    {
        var users = await _context.Users
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayName)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.DisplayName
            })
            .ToListAsync();

        var characters = await _context.Characters
            .Include(x => x.CurrentImage)
            .Where(x => x.IsActive)
            .OrderBy(x => x.PrioritySlot)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var result = users
            .Select(user => new GuildCharacterBoardItemResponse
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Characters = characters
                    .Where(character => character.UserId == user.Id)
                    .Select(Map)
                    .ToList()
            })
            .Where(x => x.Characters.Count > 0)
            .ToList();

        return result;
    }

    public async Task<List<CharacterResponse>> GetMineAsync(Guid userId, bool activeOnly)
    {
        var query = _context.Characters
            .Include(x => x.CurrentImage)
            .Where(x => x.UserId == userId);

        query = activeOnly ? query.Where(x => x.IsActive) : query.Where(x => !x.IsActive);

        return await query
            .OrderBy(x => x.PrioritySlot)
            .ThenBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync();
    }

    private static CharacterResponse Map(Character x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        CharacterClass = x.CharacterClass.ToString(),
        RoleTag = x.RoleTag.ToString(),
        PrioritySlot = x.PrioritySlot,
        LastKnownLevel = x.LastKnownLevel,
        ImageUrl = x.CurrentImage?.ImageUrl ?? string.Empty,
        IsActive = x.IsActive
    };
}
