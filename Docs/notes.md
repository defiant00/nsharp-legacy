## Items

* Expressions
    * X str literal
    * X char literal
    * X number literal
    * _ array literal
    * X null literal
    * X bool literal
    * X field/property
    * _ method call
    * _ accessor
    * _ new
    * _ new array
    * X binary op
    * _ unary op
    * _ ternary
    * _ anonymous fn
    * _ default
    * _ typeof?
* Statements
    * file level
        * X namespace
        * _ import
        * X class def
        * _ interface def
        * _ struct def
        * _ enum def
        * _ attributes
    * class level
        * X class def
        * _ interface def
        * _ struct def
        * _ enum def
        * X method def
        * _ constructor def
        * _ delegate def
        * X field def
        * X const def
        * X property def
        * _ attributes
    * method level
        * _ var
        * _ const
        * _ using
        * X assignment
        * X if/else
        * _ switch
        * _ for
        * _ foreach
        * _ try/catch/finally
        * X break
        * X continue
        * X return
        * _ base/super
        * _ attributes

## Min Syntax Notes

Attributes
```
@Special
@SpecialAttribute
@Special("val")
@assembly:Special
```

Anonymous Functions
```
fn(x) is x * x

fn(x)
    var y = x * x
    return y * 2
```

Loops
```
for i in 10
    ; 0-9

for i in myList
    ; iterate over myList

for i = 0, i < 7, i += 1
    ; normal for loop

for i < 7
    ; while loop
```

Access modifiers - sane defaults (protected, final? see kotlin) and only keywords for differences (eg, no `protected`). Maybe `open` to not be `final` but abstract are always open?

Check types with `is` and support c# single-line declaration cast check `if val is SomeClass c`  
Cast with `as`, `val as SomeClass`

Class is Parent has Interface, Interface2?

[ ] no `break` in a `switch`  
[ ] each switch case is its own scope
[ ] implicit types wherever possible
