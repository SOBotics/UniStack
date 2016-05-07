using System.Collections.Generic;

namespace UniStack
{
    public interface IBagOfWords
    {
        bool ContainsPost(uint postID);

        void AddPost(uint postID, IDictionary<string, ushort> termTFs);

        void RemovePost(uint postID, IDictionary<string, ushort> termTFs);

        Dictionary<uint, double> GetSimilarity(IDictionary<string, ushort> terms, uint maxPostsToReturn, double minSimilarity);
    }
}
