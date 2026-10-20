namespace Model
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        protected BaseEntity()
        {
            Id = 0;
        }

        protected BaseEntity(int id)
        {
            Id = id;
        }
    }
}
