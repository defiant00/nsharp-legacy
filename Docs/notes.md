## Items

* Expressions
    * _ str literal
    * _ char literal
    * _ number literal
    * _ array literal
    * _ null literal
    * _ field/property
    * _ method call
    * _ accessor
    * _ new
    * _ new array
    * _ binary op
    * _ unary op
    * _ ternary
    * _ anonymous fn
    * _ default
    * _ typeof?
* Statements
    * file level
        * _ namespace
        * _ import
        * _ class def
        * _ interface def
        * _ struct def
        * _ enum def
        * _ attributes
    * class level
        * _ class def
        * _ interface def
        * _ struct def
        * _ enum def
        * _ method def
        * _ constructor def
        * _ delegate def
        * _ field def
        * _ const def
        * _ property def
        * _ attributes
    * method level
        * _ var
        * _ const
        * _ using
        * _ assignment
        * _ if/else
        * _ switch
        * _ for
        * _ foreach
        * _ try/catch/finally
        * _ return
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

Fields (vars in classes)
```
class C
    var Name string
    var Age = 12
    private var Hidden string
```

Properties (are functions)
```
fn Name string is _name

fn As{T} T is _val as T

fn Name string
    get is _name
    set(val) is _name = val

fn Name string
    get
        return _name
    set(val)
        _name = val
```

Short form functions  
No `void`, no return type if it doesn't return anything
```
fn Test() int is 3
fn Test() is Console.WriteLine("hi")
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

types to the right
```
var x
var x int
var x = "hi"
var x = new Thing()
var x Thing = new()

fn DoThing(a int, b int) int
```

[X] No `++` and `--` or unary `+`  
[X] no syntactic sugar for string concatenation (call concat and tostring if you really need it)  
[ ] no syntactic sugar for arrays - need to check how unpleasant using the class directly is  
[ ] no `break` in a `switch`  
[ ] each switch case is its own scope
[ ] assignment is a statement  
[X] no compound assignment  
[X] no `void`  
[ ] MyClass `from` BaseClass `is` Interface1, Interface2  
[ ] `var` and `val` for mutable and immutable, `val` is `const` or `readonly` as applicable  
[ ] implicit types wherever possible
