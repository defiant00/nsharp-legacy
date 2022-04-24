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
    * _ anonymous fn
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
    ret y * 2
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

Initializers or a more generic syntax?
```
; constructor specifically
new Customer()
    FirstName = "First"
    LastName = "Last"

; more generic (applies to any expression statement or assignment?)
Store.Customer
    FirstName = "First"
    LastName = "Last"

var person = new Person()
    FirstName = "First"
    LastName = "Last"
```

Access modifiers - sane defaults (protected, final? see kotlin) and only keywords for differences (eg, no `protected`). Maybe `open` to not be `final` but abstract are always open?

[ ] no `break` in a `switch`  
[ ] each switch case is its own scope
[ ] implicit types wherever possible
