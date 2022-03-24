# Neutral Programming

Neutral programming is a simple concept - as a developer I care about programming constructs: things like loops, classes, and functions. What I _don't_ care about is how others create those things. It shouldn't matter to me how you format your code, or even what language you use. And by the same token, I should be able to see and edit the code as I prefer, without affecting my ability to collaborate with others.

We achieve this with two main steps. First is an enhanced abstract syntax tree that keeps comments and formatting information. This way we can store programming constructs in a language-independent way. The second piece is a few simple requirements that all language processors must comply with:

* Given an AST, the language processor will generate source code following the user's formatting preferences.
* Given the generated source code and the user's formatting preferences, the language processor will generate an AST.
* As long as the user's formatting preferences stay the same, the generated AST will be _identical_ to the original input.

This means that a project repository can have a defined language and formatting preferences, and users can have their own as well. Files can be converted to a user's preference to start editing, and converted back to the project's on save. Since formatting and one-to-one conversion is guaranteed, the project owner sees only the changes they actually care about - the programming constructs themselves - and no longer has to worry about a number of common potential issues such as indentation.

Beyond basic formatting, this also means that users can program in entirely different styles as long as the language supports the same basic programming constructs. Both `for (int i = 0; i < 10; i++)` and `for i in range(10)` represent the same underlying construct, so you should be able to use whichever you prefer.