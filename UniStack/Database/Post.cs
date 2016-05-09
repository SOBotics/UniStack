using System.Collections.Generic;

namespace UniStack.Database
{
    public class Post
    {
        public int PostID { get; set; }
        public float Length { get; set; }
        public string Tags { get; set; }
        public virtual ICollection<Term> Terms { get; set; }
    }
}
