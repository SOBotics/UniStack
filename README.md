# UniStack

## Current algorithm (v1)

The current algorithm only applies to "popular" tags (the number of tags supported will depend on your hardware).

### To search for duplicates

 1. Tokenise the source question's body (this is the question which we will base our duplicate search on) into "tags" which represent certain non-plain-text characteristics of the post (such as: links, pictures, quotes, code blocks, etc.).
 
 2. Use a PoS tagger to tag the remaining string.
 
 3. Generate a term (or word)/frequency dictionary from the tagged string.
 
 4. Take the source question's most popular tag used and consult the bag of words object associated to said tag for a cosine similarity score (if no tag is found we can assume the question is not a duplicate).
 
 5. If the similarity is below *x*% we can assume the question is not a duplicate.
 
### To initialise the search engine

 1. Take a list of the top *x* most popular tags on Stack Overflow.
 
 2. For each tag:
 
  I. Create a new bag of words object.
  
  II. Add every post in the tag to the object's "bag".
