# UniStack

## Current algorithm (v1.1)

The current algorithm only applies to "popular" tags (the number of tags supported will depend on your hardware, i.e., RAM).

### To search for duplicates

 1. Tokenise the source question's body (this is the question which we will base our duplicate search on) into "tags" which represent certain non-plain-text characteristics of the post (such as: links, pictures, quotes, code blocks, etc.).
 
 2. Split the tagged text into an array of words (whilst removing punctuation and whitespace).
 
 3. Generate a word/frequency dictionary from the array of words.
 
 4. Take the source question's most popular tag used and consult the bag of words object associated to said tag for a cosine similarity score (if no tag is found we can assume the question is not a duplicate).
 
 5. If the similarity is above a threshold of *x*% the question can be classified as a duplicate.
 
### To initialise the search engine

 1. Take a list of the top *x* most popular tags on Stack Overflow.
 
 2. For each tag:
 
  I. Create a new bag of words object.
  
  II. Add every post in the tag to the object's "bag".
