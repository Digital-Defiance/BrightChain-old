using System.Linq;
using BrightChain.Engine.Quorum.BackblazeReedSolomon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Engine.Tests;

/// <summary>
///     This is a totally paranoid test that ensure that the Galois class
///     actually implements a field, with all of the properties that a field
///     must have.
/// </summary>
[TestClass]
public class BackblazeGaloisTest
{
    [TestMethod]
    public void testClosure()
    {
        // Unlike the Python implementation, there is no need to test
        // for closure.  Because add(), subtract(), multiply(), and
        // divide() all return bytes, there's no way they could
        // possible return something outside the field.
    }

    [TestMethod]
    public void testAssociativity()
    {
        for (var i = -128; i < 128; i++)
        {
            var a = (byte)i;
            for (var j = -128; j < 128; j++)
            {
                var b = (byte)j;
                for (var k = -128; k < 128; k++)
                {
                    var c = (byte)k;
                    Assert.Equals(
                        objA: Galois.add(a: a,
                            b: Galois.add(a: b,
                                b: c)),
                        objB: Galois.add(a: Galois.add(a: a,
                                b: b),
                            b: c)
                    );
                    Assert.Equals(
                        objA: Galois.multiply(a: a,
                            b: (byte)Galois.multiply(a: b,
                                b: c)),
                        objB: Galois.multiply(a: (byte)Galois.multiply(a: a,
                                b: b),
                            b: c)
                    );
                }
            }
        }
    }

    [TestMethod]
    public void testIdentity()
    {
        for (var i = -128; i < 128; i++)
        {
            var a = (byte)i;
            Assert.Equals(objA: a,
                objB: Galois.add(a: a,
                    b: 0));
            Assert.Equals(objA: a,
                objB: Galois.multiply(a: a,
                    b: 1));
        }
    }

    [TestMethod]
    public void testInverse()
    {
        for (var i = -128; i < 128; i++)
        {
            sbyte a = (sbyte)i;
            {
                var b = Galois.subtract(a: 0,
                    b: (byte) a);
                Assert.Equals(objA: 0,
                    objB: Galois.add(a: (byte) a,
                        b: b));
            }
            if (a != 0)
            {
                byte b = (byte)Galois.divide(
                    a: 1,
                    b: a);
                Assert.Equals(objA: 1,
                    objB: Galois.multiply(a: (byte) a,
                        b: b));
            }
        }
    }

    [TestMethod]
    public void testCommutativity()
    {
        for (var i = -128; i < 128; i++)
        {
            for (var j = -128; j < 128; j++)
            {
                var a = (byte)i;
                var b = (byte)j;
                Assert.Equals(objA: Galois.add(a: a,
                        b: b),
                    objB: Galois.add(a: b,
                        b: a));
                Assert.Equals(objA: Galois.multiply(a: a,
                        b: b),
                    objB: Galois.multiply(a: b,
                        b: a));
            }
        }
    }

    [TestMethod]
    public void testDistributivity()
    {
        for (var i = -128; i < 128; i++)
        {
            var a = (byte)i;
            for (var j = -128; j < 128; j++)
            {
                var b = (byte)j;
                for (var k = -128; k < 128; k++)
                {
                    var c = (byte)k;
                    Assert.Equals(
                        objA: Galois.multiply(a: a,
                            b: Galois.add(a: b,
                                b: c)),
                        objB: Galois.add(a: (byte)Galois.multiply(a: a,
                                b: b),
                            b: (byte)Galois.multiply(a: a,
                                b: c))
                    );
                }
            }
        }
    }

    [TestMethod]
    public void testExp()
    {
        for (var i = -128; i < 128; i++)
        {
            var a = (byte)i;
            short power = 1;
            for (var j = 0; j < 256; j++)
            {
                Assert.Equals(objA: power,
                    objB: Galois.exp(a: a,
                        n: j));
                power = Galois.multiply(a: (byte)power,
                    b: a);
            }
        }
    }

    [TestMethod]
    public void testGenerateLogTable()
    {
        var logTable = Galois.generateLogTable(polynomial: Galois.GENERATING_POLYNOMIAL);
        Assert.IsTrue(condition: Galois.LOG_TABLE.SequenceEqual(second: logTable));

        var expTable = Galois.generateExpTable(logTable: logTable);
        Assert.IsTrue(condition: Galois.EXP_TABLE.SequenceEqual(second: expTable));

        int[] polynomials = {29, 43, 45, 77, 95, 99, 101, 105, 113, 135, 141, 169, 195, 207, 231, 245};
        Assert.IsTrue(condition: polynomials.SequenceEqual(second: Galois.allPossiblePolynomials()));
    }

    [TestMethod]
    public void testMultiplicationTable()
    {
        var table = Galois.MULTIPLICATION_TABLE;
        for (var a = -128; a < 128; a++)
        {
            for (var b = -128; b < 128; b++)
            {
                Assert.Equals(objA: Galois.multiply(a: (byte)a,
                        b: (byte)b),
                    objB: table[a & 0xFF,
                        b & 0xFF]);
            }
        }
    }

    [TestMethod]
    public void testWithPythonAnswers()
    {
        // These values were copied output of the Python code.
        Assert.Equals(objA: 12,
            objB: Galois.multiply(a: 3,
                b: 4));
        Assert.Equals(objA: 21,
            objB: Galois.multiply(a: 7,
                b: 7));
        Assert.Equals(objA: 41,
            objB: Galois.multiply(a: 23,
                b: 45));

        Assert.Equals(objA: (byte)4,
            objB: Galois.exp(a: 2,
                n: 2));
        Assert.Equals(objA: (byte)235,
            objB: Galois.exp(a: 5,
                n: 20));
        Assert.Equals(objA: (byte)43,
            objB: Galois.exp(a: 13,
                n: 7));
    }
}
