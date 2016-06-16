# UniStack

## Current algorithm (v1.3)

The current algorithm only applies to "popular" tags (the number of tags supported will depend on your hardware, i.e., RAM).

### Generating question models 

 1. Tokenise the question's *body* (including html, leave the title, tags, etc.) into "tags" which will represent non-plain-text characteristics of the post, currently: links, pictures, code blocks and inline code (backticked text).
 
  I. Tag links and pictures with `•L•` and `•P•` respectively.

  II. Tag inline code with: `•ICS•` for content 10 characters or less, `•ICM•` for content 35 characters or less, and `•ICL•` for content greater than 35 characters.

  III. Tag code blocks with: `•CBS•` for content 5 lines or less, `•CBM•` for content 20 lines or less, and `•CBL•` for content greater than 20 lines.
 
 2. Split the tagged text into an array of sentences (or lines).

 3. Calculate each sentence's punctuation ratio (totalPuncChars / totalNonPuncChars [excluding whitespace]) and remove any items scoring greater than 10% (ignoring tags).

 4. Split the items into an array of words (whilst removing punctuation and whitespace).
 
 5. Hash each word to generate a new array.
 
 5. Generate a dictionary from the array of hashed words (where the key is the hash, and the value is the number of occurrences of the hash).

 6. This dictionary is now your model of the question.

### To search for duplicates
 
 1. Fetch the source question's *body* (including HTML) and the most popular tag used, leave everything else.

 2. Generate a model of the question.
 
 7. Take the source question's first tag and consult the bag of words object associated to said tag for a cosine similarity score (if there is no such object for the tag, assume the question is not a duplicate) of the model.
 
 5. If the similarity is above a threshold of ~12.5% (this number requires experimentation for optimal accuracy) the question can be classified as a duplicate.
 
### To initialise the search engine

 1. Take a list of the top *x* most popular tags on Stack Overflow.
 
 2. For each tag:
 
  I. Create a new bag of words object.
  
  II. Generate a model for each post in the tag and add it to the object's "bag".
