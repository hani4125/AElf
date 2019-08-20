namespace AElf.Dtos
{
    public class TransactionResultDto
    {
        public string TransactionId { get; set; }
        
        public string Status { get; set; }
        
        public LogEventDto[] Logs { get; set; }
        
        public string Bloom { get; set; }
         
        public long BlockNumber { get; set; }
        
        public string BlockHash { get; set; }
        
        public TransactionDto Transaction { get; set; }
        
        public string ReadableReturnValue { get; set; }
        
        public string Error { get; set; }
    }
}