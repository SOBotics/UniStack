# UniStack

An experimental duplicate question search algorithm for Stack Exchange.

# Current Algorithm

*As this is an experimental project, everything is subject to change.*

Extract question sentence + context words, then model remianing words.

 - Question sentence: words starting from an MD or Wxx tagged word till the end of the sentence. 
 - Context words: any NN tagged word.
 - Model: PoS tagged bag-of-words (unigram).
 - Weighting: TF-IDF.
 - Similarity function: cosine.
