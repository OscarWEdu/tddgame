namespace TddGame;

public interface IContinentRepository
{
    Task<IEnumerable<ContinentDto>> GetContinentsAsync(CancellationToken ct);

    Task<ContinentDto> CreateContinentAsync(string continentName, int bonus, CancellationToken ct);
}