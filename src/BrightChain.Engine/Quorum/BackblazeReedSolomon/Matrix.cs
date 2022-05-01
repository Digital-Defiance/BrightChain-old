using System;
using System.Text;

namespace BrightChain.Engine.Quorum.BackblazeReedSolomon;

/// <summary>
///     A matrix over the 8-bit Galois field.
///     This class is not performance-critical, so the implementations
///     are simple and straightforward.
/// </summary>
public class Matrix
{
    /// <summary>
    ///     The number of columns in the matrix.
    /// </summary>
    private readonly int columns;

    /// <summary>
    ///     The data in the matrix, in row major form.
    ///     To get element (r, c): data[r][c]
    ///     Because this this is computer science, and not math,
    ///     the indices for both the row and column start at 0.
    /// </summary>
    private readonly int[][] data;

    /// <summary>
    ///     The number of rows in the matrix.
    /// </summary>
    private readonly int rows;


    /// <summary>
    ///     Initializes a new instance of the <see cref="Matrix" /> class full of zeroes.
    /// </summary>
    /// <param name="initRows">The number of rows in the matrix.</param>
    /// <param name="initColumns">The number of columns in the matrix.</param>
    public Matrix(int initRows, int initColumns)
    {
        this.rows = initRows;
        this.columns = initColumns;
        this.data = new int [this.rows][];
        for (var r = 0; r < this.rows; r++)
        {
            this.data[r] = new int[this.columns];
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Matrix" /> class with the given row-major data
    /// </summary>
    /// <param name="initData"></param>
    /// <exception cref="ArgumentException"></exception>
    public Matrix(byte[][] initData)
    {
        this.rows = initData.Length;
        this.columns = initData[0].Length;
        this.data = new int[this.rows][];
        for (var r = 0; r < this.rows; r++)
        {
            if (initData[r].Length != this.columns)
            {
                throw new ArgumentException(message: "Not all rows have the same number of columns");
            }

            this.data[r] = new int[this.columns];
            for (var c = 0; c < this.columns; c++)
            {
                this.data[r][c] = initData[r][c];
            }
        }
    }

    /// <summary>
    ///     Returns an identity matrix of the given size.
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static Matrix identity(int size)
    {
        var result = new Matrix(
            initRows: size,
            initColumns: size);
        for (var i = 0; i < size; i++)
        {
            result.set(
                r: i,
                c: i,
                value: 1);
        }

        return result;
    }

    /// <summary>
    ///     Returns a human-readable string of the matrix contents.
    ///     Example: [[1, 2], [3, 4]]
    /// </summary>
    /// <returns>string.</returns>
    public override string ToString()
    {
        var result = new StringBuilder();
        result.Append(value: '[');
        for (var r = 0; r < this.rows; r++)
        {
            if (r != 0)
            {
                result.Append(value: ", ");
            }

            result.Append(value: '[');
            for (var c = 0; c < this.columns; c++)
            {
                if (c != 0)
                {
                    result.Append(value: ", ");
                }

                result.Append(value: this.data[r][c] & 0xFF);
            }

            result.Append(value: ']');
        }

        result.Append(value: ']');
        return result.ToString();
    }


    /// <summary>
    ///     Returns a human-readable string of the matrix contents.
    ///     Example:
    ///     00 01 02
    ///     03 04 05
    ///     06 07 08
    ///     09 0a 0b
    /// </summary>
    /// <returns></returns>
    public string toBigString()
    {
        var result = new StringBuilder();
        for (var r = 0; r < this.rows; r++)
        {
            for (var c = 0; c < this.columns; c++)
            {
                var value = this.get(
                    r: r,
                    c: c);
                if (value < 0)
                {
                    value += 256;
                }

                result.Append(value: string.Format(
                    format: "%02x ",
                    arg0: value));
            }

            result.Append(value: "\n");
        }

        return result.ToString();
    }

    /// <summary>
    ///     Returns the number of columns in this matrix.
    /// </summary>
    /// <returns></returns>
    public int getColumns()
    {
        return this.columns;
    }

    /// <summary>
    ///     Returns the number of rows in this matrix.
    /// </summary>
    /// <returns></returns>
    public int getRows()
    {
        return this.rows;
    }

    /// <summary>
    ///     Returns the value at row r, column c.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int get(int r, int c)
    {
        if (r < 0 || this.rows <= r)
        {
            throw new ArgumentOutOfRangeException(paramName: "Row index out of range: " + r);
        }

        if (c < 0 || this.columns <= c)
        {
            throw new ArgumentOutOfRangeException(paramName: "Column index out of range: " + c);
        }

        return this.data[r][c];
    }

    /// <summary>
    ///     Sets the value at row r, column c.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void set(int r, int c, int value)
    {
        if (r < 0 || this.rows <= r)
        {
            throw new ArgumentOutOfRangeException(paramName: "Row index out of range: " + r);
        }

        if (c < 0 || this.columns <= c)
        {
            throw new ArgumentOutOfRangeException(paramName: "Column index out of range: " + c);
        }

        this.data[r][c] = value;
    }

    /// <summary>
    ///     Returns true iff this matrix is identical to the other.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool equals(object other)
    {
        if (!(other is Matrix))
        {
            return false;
        }

        for (var r = 0; r < this.rows; r++)
        {
            if (!Equals(objA: this.data[r],
                    objB: ((Matrix)other).data[r]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Multiplies this matrix (the one on the left) by another
    ///     matrix (the one on the right).
    /// </summary>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Matrix times(Matrix right)
    {
        if (this.getColumns() != right.getRows())
        {
            throw new ArgumentOutOfRangeException(
                paramName: "Columns on left (" + this.getColumns() + ") " +
                           "is different than rows on right (" + right.getRows() + ")");
        }

        var result = new Matrix(initRows: this.getRows(),
            initColumns: right.getColumns());
        for (var r = 0; r < this.getRows(); r++)
        {
            for (var c = 0; c < right.getColumns(); c++)
            {
                var value = 0;
                for (var i = 0; i < this.getColumns(); i++)
                {
                    value ^= Galois.multiply(
                        a: (byte) this.get(
                            r: r,
                            c: i),
                        b: (byte) right.get(
                            r: i,
                            c: c));
                }

                result.set(
                    r: r,
                    c: c,
                    value: value);
            }
        }

        return result;
    }

    /**
     * Returns the concatenation of this matrix and the matrix on the right.
     */
    public Matrix augment(Matrix right)
    {
        if (this.rows != right.rows)
        {
            throw new ArgumentOutOfRangeException(paramName: "Matrices don't have the same number of rows");
        }

        var result = new Matrix(initRows: this.rows,
            initColumns: this.columns + right.columns);
        for (var r = 0; r < this.rows; r++)
        {
            for (var c = 0; c < this.columns; c++)
            {
                result.data[r][c] = this.data[r][c];
            }

            for (var c = 0; c < right.columns; c++)
            {
                result.data[r][this.columns + c] = right.data[r][c];
            }
        }

        return result;
    }

    /**
     * Returns a part of this matrix.
     */
    public Matrix submatrix(int rmin, int cmin, int rmax, int cmax)
    {
        var result = new Matrix(initRows: rmax - rmin,
            initColumns: cmax - cmin);
        for (var r = rmin; r < rmax; r++)
        {
            for (var c = cmin; c < cmax; c++)
            {
                result.data[r - rmin][c - cmin] = this.data[r][c];
            }
        }

        return result;
    }

    /**
     * Returns one row of the matrix as a byte array.
     */
    public int[] getRow(int row)
    {
        var result = new int[this.columns];
        for (var c = 0; c < this.columns; c++)
        {
            result[c] = this.get(
                r: row,
                c: c);
        }

        return result;
    }

    /**
     * Exchanges two rows in the matrix.
     */
    public void swapRows(int r1, int r2)
    {
        if (r1 < 0 || this.rows <= r1 || r2 < 0 || this.rows <= r2)
        {
            throw new ArgumentOutOfRangeException(paramName: "Row index out of range");
        }

        var tmp = this.data[r1];
        this.data[r1] = this.data[r2];
        this.data[r2] = tmp;
    }

    /// <summary>
    ///     Returns the inverse of this matrix.
    ///     @throws IllegalArgumentException when the matrix is singular and
    ///     doesn't have an inverse.
    /// </summary>
    public Matrix invert()
    {
        // Sanity check.
        if (this.rows != this.columns)
        {
            throw new ArgumentOutOfRangeException(paramName: "Only square matrices can be inverted");
        }

        // Create a working matrix by augmenting this one with
        // an identity matrix on the right.
        var work = this.augment(right: identity(size: this.rows));

        // Do Gaussian elimination to transform the left half into
        // an identity matrix.
        work.gaussianElimination();

        // The right half is now the inverse.
        return work.submatrix(
            rmin: 0,
            cmin: this.rows,
            rmax: this.columns,
            cmax: this.columns * 2);
    }

    /// <summary>
    ///     Does the work of matrix inversion.
    ///     Assumes that this is an r by 2r matrix.
    /// </summary>
    private void gaussianElimination()
    {
        // Clear out the part below the main diagonal and scale the main
        // diagonal to be 1.
        for (var r = 0; r < this.rows; r++)
        {
            // If the element on the diagonal is 0, find a row below
            // that has a non-zero and swap them.
            if (this.data[r][r] == 0)
            {
                for (var rowBelow = r + 1; rowBelow < this.rows; rowBelow++)
                {
                    if (this.data[rowBelow][r] != 0)
                    {
                        this.swapRows(
                            r1: r,
                            r2: rowBelow);
                        break;
                    }
                }
            }

            // If we couldn't find one, the matrix is singular.
            if (this.data[r][r] == 0)
            {
                throw new ArgumentOutOfRangeException(paramName: "Matrix is singular");
            }

            // Scale to 1.
            if (this.data[r][r] != 1)
            {
                var scale = Galois.divide(
                    a: 1,
                    b: (sbyte) this.data[r][r]);
                for (var c = 0; c < this.columns; c++)
                {
                    this.data[r][c] = Galois.multiply(
                        a: (byte) this.data[r][c],
                        b: (byte) scale);
                }
            }

            // Make everything below the 1 be a 0 by subtracting
            // a multiple of it.  (Subtraction and addition are
            // both exclusive or in the Galois field.)
            for (var rowBelow = r + 1; rowBelow < this.rows; rowBelow++)
            {
                if (this.data[rowBelow][r] != 0)
                {
                    var scale = this.data[rowBelow][r];
                    for (var c = 0; c < this.columns; c++)
                    {
                        this.data[rowBelow][c] ^= Galois.multiply(
                            a: (byte) scale,
                            b: (byte) this.data[r][c]);
                    }
                }
            }
        }

        // Now clear the part above the main diagonal.
        for (var d = 0; d < this.rows; d++)
        {
            for (var rowAbove = 0; rowAbove < d; rowAbove++)
            {
                if (this.data[rowAbove][d] != 0)
                {
                    var scale = this.data[rowAbove][d];
                    for (var c = 0; c < this.columns; c++)
                    {
                        this.data[rowAbove][c] ^= Galois.multiply(
                            a: (byte) scale,
                            b: (byte) this.data[d][c]);
                    }
                }
            }
        }
    }
}
