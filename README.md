# Burst Diverge Test
Small example to demonstrate that constant folding isn't consistent between Burst and non-Burst.

## Example
This method will give different outputs on Burst / non-Burst, depending on the input:

```csharp
private static float Calculate(float input)
{
    return 0.1f * (input + 0.1f);
}
```

Here's an example of diverging outputs for a given input:

```
           Input: +0.6176773000 (3f1e201a)
non-Burst Output: +0.0717677300 (3d92faf6)
    Burst Output: +0.0717677400 (3d92faf7)
```

## Unity Versions
So far this has been reproduced on:

* 2019.4.10f1
* 2020.3.11f1
