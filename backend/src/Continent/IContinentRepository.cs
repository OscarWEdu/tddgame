namespace TddGame;

public interface IContinentRepository
{
    Task<IEnumerable<ContinentDto>> GetContinentsAsync(CancellationToken ct);
}