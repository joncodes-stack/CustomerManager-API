namespace CustomerManager.Infra.Messaging.Event
{
    public record CustomerEvent
    {
        public string TipoEvento { get; init; } // "ContaCriada" | "ContaAtualizada" | "ContaDeletada"
        public string CustomerId { get; init; }
        public DateTime OcorridoEm { get; init; }
    }
}
