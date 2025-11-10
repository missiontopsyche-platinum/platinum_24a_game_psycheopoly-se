public interface ICardEventPublisher
{
    void Publish<T>(T payload);
}