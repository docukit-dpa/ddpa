namespace DDPA.DTO
{
    public class DatasetFieldDTO
    {
        public int Id { get; set; }

        public int DatasetId { get; set; }

        public int FieldId { get; set; }

        public int Order { get; set; }

        public FieldDTO Field { get; set; }
    }
}
