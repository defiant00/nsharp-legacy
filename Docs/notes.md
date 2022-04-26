## Items

* Expressions
    * X str literal
    * X char literal
    * X number literal
    * _ array literal
    * X null literal
    * X bool literal
    * X field/property
    * X method call
    * X accessor
    * X new
    * _ new array
    * X binary op
    * X unary op
    * X is op
    * _ ternary
    * X anonymous fn
    * _ default
    * _ typeof?
* Statements
    * file level
        * X namespace
        * X import
        * X class def
        * X interface def
        * _ struct def
        * X enum def
        * X delegate def
        * _ attributes
    * class level
        * X class def
        * X interface def
        * _ struct def
        * X enum def
        * X method def
        * X constructor def
        * X delegate def
        * X field def
        * X const def
        * X property def
        * _ attributes
    * method level
        * X var
        * X const
        * X using
        * X assignment
        * X if/else
        * X switch
        * _ for
        * _ foreach
        * X try/catch/finally
        * X break
        * X continue
        * X return
        * _ base/super
        * _ attributes
        * _ throw/rethrow

## Min Syntax Notes

Attributes
```
@Special
@SpecialAttribute
@Special("val")
@assembly:Special
```

Loops
```
for i in 10
    ; 0-9

for i in myList
    ; iterate over myList

for i in myList
    Console.Write(i)
between/div/sep
    Console.Write(", ")

for i = 0, i < 7, i += 1
    ; normal for loop

for i < 7
    ; while loop
```

Access modifiers - sane defaults (protected, final? see kotlin) and only keywords for differences (eg, no `protected`). Maybe `open` to not be `final` but abstract are always open?


implicit types wherever possible
