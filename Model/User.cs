namespace Model
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }

        public User() : base()
        {
            Username = string.Empty;
            PasswordHash = string.Empty;
            RoleID = 2; // ברירת מחדל לתלמיד (Student) למשל
            RoleName = "User";
            IsActive = true;
        }

        public User(int id, string username, string passwordHash, int roleID, string roleName, bool isActive) : base(id)
        {
            Username = username;
            PasswordHash = passwordHash;
            RoleID = roleID;
            RoleName = roleName;
            IsActive = isActive;
        }

        public override string ToString()
        {
            return $"User: {Username} (Role: {RoleName}, Active: {IsActive})";
        }
    }
}