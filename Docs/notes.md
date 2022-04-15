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
fn(x) = x * x

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
fn Name string = _name

fn As{T} T = _val as T

fn Name string
    get = _name
    set(val) = _name = val

fn Name string
    get
        return _name
    set(val)
        _name = val
```

Short form functions  
No `void`, no return type if it doesn't return anything
```
fn Test() int = 3
fn Test() = Console.WriteLine("hi")
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
[ ] no syntactic sugar for string concatenation (call concat and tostring if you really need it)  
[ ] no `break` in a `switch`  
[ ] each switch case is its own scope
[ ] assignment is a statement  
[X] no compound assignment  
[X] no `void`  
[ ] MyClass `from` BaseClass `is` IEnumerable  
[ ] local readonly (or const works for runtime as well?)
[ ] const by default? or have `var` and `val` for mutable and immutable? `mut` and `val`?