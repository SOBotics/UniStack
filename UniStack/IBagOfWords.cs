using System.Collections.Generic;

namespace UniStack
{
    public interface IBagOfWords
    {
        bool ContainsPost(uint postID);

        void AddPost(uint postID, IDictionary<string, uint> termTFs);

        void RemovePost(uint postID, IDictionary<string, uint> termTFs);

        Dictionary<uint, double> GetSimilarity(IDictionary<string, uint> terms, uint maxPostsToReturn);
    }
}
