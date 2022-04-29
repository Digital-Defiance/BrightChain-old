using System;
using System.Linq;
using BrightChain.Engine.Quorum.BackblazeReedSolomon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Engine.Tests;

/// <summary>
/// This is a totally paranoid test that ensure that the Galois class
/// actually implements a field, with all of the properties that a field
/// must have.
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
    public void testAssociativity() {
        for (int i = -128; i < 128; i++) {
            byte a = (byte)i;
            for (int j = -128; j < 128; j++) {
                byte b = (byte) j;
                for (int k = -128; k < 128; k++) {
                    byte c = (byte) k;
                    Assert.Equals(
                            Galois.add(a, Galois.add(b, c)),
                            Galois.add(Galois.add(a, b), c)
                    );
                    Assert.Equals(
                            Galois.multiply(a, (byte)Galois.multiply(b, c)),
                            Galois.multiply((byte)Galois.multiply(a, b), c)
                    );
                }
            }
        }
    }

    [TestMethod]
    public void testIdentity() {
        for (int i = -128; i < 128; i++) {
            byte a = (byte) i;
            Assert.Equals(a, Galois.add(a, (byte) 0));
            Assert.Equals(a, Galois.multiply(a, (byte) 1));
        }
    }

    [TestMethod]
    public void testInverse() {
        for (int i = -128; i < 128; i++) {
            byte a = (byte) i;
            {
                byte b = Galois.subtract((byte) 0, a);
                Assert.Equals(0, Galois.add(a, b));
            }
            if (a != 0) {
                byte b = Galois.divide((byte) 1, a);
                Assert.Equals(1, Galois.multiply(a, b));
            }
        }
    }

    [TestMethod]
    public void testCommutativity() {
        for (int i = -128; i < 128; i++) {
            for (int j = -128; j < 128; j++) {
                byte a = (byte) i;
                byte b = (byte) j;
                Assert.Equals(Galois.add(a, b), Galois.add(b, a));
                Assert.Equals(Galois.multiply(a, b), Galois.multiply(b, a));
            }
        }
    }

    [TestMethod]
    public void testDistributivity() {
        for (int i = -128; i < 128; i++) {
            byte a = (byte) i;
            for (int j = -128; j < 128; j++) {
                byte b = (byte) j;
                for (int k = -128; k < 128; k++) {
                    byte c = (byte) k;
                    Assert.Equals(
                            Galois.multiply(a, Galois.add(b, c)),
                            Galois.add(Galois.multiply(a, b), Galois.multiply(a, c))
                    );
                }
            }
        }
    }

    [TestMethod]
    public void testExp() {
        for (int i = -128; i < 128; i++) {
            byte a = (byte) i;
            byte power = 1;
            for (int j = 0; j < 256; j++) {
                Assert.Equals(power, Galois.exp(a, j));
                power = Galois.multiply(power, a);
            }
        }
    }

    [TestMethod]
    public void testGenerateLogTable() {
        short[] logTable = Galois.generateLogTable(Galois.GENERATING_POLYNOMIAL);
        Assert.IsTrue(Galois.LOG_TABLE.SequenceEqual(logTable));

        sbyte [] expTable = Galois.generateExpTable(logTable);
        assertArrayEquals(Galois.EXP_TABLE, expTable);

        final Integer [] polynomials = {
                29, 43, 45, 77, 95, 99, 101, 105, 113,
                135, 141, 169, 195, 207, 231, 245
        };
        assertArrayEquals(polynomials, Galois.allPossiblePolynomials());
    }

    [TestMethod]
    public void testMultiplicationTable() {
        byte [] [] table = Galois.MULTIPLICATION_TABLE;
        for (int a = -128; a < 128; a++) {
            for (int b = -128; b < 128; b++) {
                Assert.Equals(Galois.multiply((byte) a, (byte) b), table[a & 0xFF][b & 0xFF]);
            }
        }
    }

    [TestMethod]
    public void testWithPythonAnswers() {
        // These values were copied output of the Python code.
        Assert.Equals(12, Galois.multiply((byte)3, (byte)4));
        Assert.Equals(21, Galois.multiply((byte)7, (byte)7));
        Assert.Equals(41, Galois.multiply((byte)23, (byte)45));

        Assert.Equals((byte)   4, Galois.exp((byte) 2, (byte) 2));
        Assert.Equals((byte) 235, Galois.exp((byte) 5, (byte) 20));
        Assert.Equals((byte)  43, Galois.exp((byte) 13, (byte) 7));
    }
}
