# Number Fields

If you need to collect numeric data then that will automatically be handled for you with a HTML5 `<input type="number">` field.

Any one of the following model types will trigger one of these fields:

```cs
public byte ByteField { get; set; }
public sbyte SbyteField { get; set; }
public short ShortField { get; set; }
public ushort UshortField { get; set; }
public int IntField { get; set; }
public uint UintField { get; set; }
public long LongField { get; set; }
public ulong UlongField { get; set; }
public float FloatField { get; set; }
public double DoubleField { get; set; }
public decimal DecimalField { get; set; }
```

## Default HTML

### Integral types

For integral types (`byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, or `ulong`), when using the Default Field Generator then the default HTML of the [Field Element](field-element.md) will be:

```html
<input type="number" step="1" %validationAttrs% %htmlAttributes% id="%propertyName%" name="%propertyName%" required="required" value="%value%" />
```

### Floating-point types

For floating-point types (`float`, `double`, or `decimal`), when using the Default Field Generator then the default HTML of the [Field Element](field-element.md) will be:

```html
<input type="number" step="any" %validationAttrs% %htmlAttributes% id="%propertyName%" name="%propertyName%" required="required" value="%value%" />
```

### Default min / max

The `min` and `max` attributes will automatically be set in the following instances:

* `byte`: `min="0" max="255"`
* `sbyte`: `min="-128" max="127"`
* `short`: `min="-32768" max="32767"`
* `ushort`: `min="0" max="65535"`
* `uint`: `min="0"`
* `ulong`: `min="0"`

## Configurability

### Specify currency step

You can specify your property as a currency value, which will automatically set the `step` attribute for you to `0.01`:

```cs
[DataType(DataType.Currency)]
public decimal DecimalField { get; set; }
```

### Specify step

You can easily specify the `step` HTML attribute by using the `Step` method on the [Field Configuration](field-configuration.md), e.g.:

# [Tag Helpers variant](#tab/step-th)

```cshtml
<field for="IntField" step="2" />
@* or *@
<field for="IntField" fluent-config='c => c.Step(2)' />
```

# [HTML Helpers variant](#tab/step-hh)

```cshtml
@s.FieldFor(m => m.IntField).Step(2)
```

***

If you specify this, it will override any default step value.

### Specify min and max

You can easily specify the `min` and `max` HTML attributes by using the `Min` and `Max` methods on the [Field Configuration](field-configuration.md), e.g.:

# [Tag Helpers variant](#tab/min-max-th)

```cshtml
<field for="IntField" min="2" max="60" />
<field for="DecimalField" min="1.1" max="1.9" />
@* or *@
<field for="IntField" fluent-config='c => c.Min(2).Max(60)' />
<field for="DecimalField" fluent-config='c => c.Min(1.1m).Max(1.9m)' />
```

# [HTML Helpers variant](#tab/min-max-hh)

```cshtml
@s.FieldFor(m => m.IntField).Min(5).Max(60)
@s.FieldFor(m => m.DecimalField).Min(1.1m).Max(1.9m)
```

***

If you specify these, it will override any default min/max values.
