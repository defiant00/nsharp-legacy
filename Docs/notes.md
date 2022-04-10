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
fn(x) x * x
fn(x)
    var y = x * x
    return y * 2

string Name _name

T As{T} _val as T

string Name
    get _name
    set(val) _name = val

string Name
    get
        return _name
    set(val)
        _name = val
```

Loops
```
for i in 10
    ; 0-9

for i in myList
    ; iterate over myList

for i = 0, i < 7, i++
    ; normal for loop

for i < 7
    ; while loop
```

Access modifiers - sane defaults (protected, final? see kotlin) and only keywords for differences (eg, no `protected`). Maybe `open` to not be `final` but abstract are always open?

Check types with `is` and support c# single-line declaration cast check `if val is SomeClass c`  
Cast with `as`, `val as SomeClass`