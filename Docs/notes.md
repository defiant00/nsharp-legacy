## TODO

* Strings and chars unescape on parse, escape on output
* Creating generics on classes/methods/etc?

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
    * X conditional
    * X anonymous fn
    * _ default
    * _ typeof?
* Statements
    * file level
        * X namespace
        * X import
        * X class def
        * X interface def
        * X struct def
        * X enum def
        * X delegate def
        * _ attributes
    * class level
        * X class def
        * X interface def
        * X struct def
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
        * X for
        * X foreach
        * X try/catch/finally
        * X break
        * X continue
        * X return
        * X base
        * _ attributes
        * X throw
        * _ rethrow

## Min Syntax Notes

Attributes
```
@Special
@SpecialAttribute
@Special("val")
@assembly:Special
```

Access modifiers - sane defaults (protected, final? see kotlin) and only keywords for differences (eg, no `protected`). Maybe `open` to not be `final` but abstract are always open?


implicit types wherever possible
