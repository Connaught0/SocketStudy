using System;
using UnityEngine.PlayerLoop;

public class ByteArray
{
    /*
     | read | ready to read|
     [][][][][][][][][][][][][][][][][][][][]
             |  -Length-  |    -remain-     |
           readidx     writeidx          capacity
     */

    const int DEFAULT_SIZE = 1024;
    int initSize = 0;


    public byte[] bytes;
    public int readIdx = 0;
    private int capacity = 0;
    private int remain { get { return capacity - writeIdx; } }

    public int writeIdx = 0;
    public int length
    {
        get { return writeIdx-readIdx; }
    }
    public ByteArray(int size=DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }
    public ByteArray(byte[] defaultBytes )
    {
        this.bytes = defaultBytes;
        readIdx = 0;
        capacity = defaultBytes.Length;
        initSize=defaultBytes.Length;
        writeIdx = defaultBytes.Length;
    }
    public void ReSize(int size)
    {
        if (size < length) return;
        if (size < initSize) return;
        int n = 1;
        while (n < size) n *= 2;
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy ( bytes, readIdx, newBytes, 0, writeIdx - readIdx );
        bytes = newBytes;
        writeIdx = length;
        readIdx=0;
    }
    public void CheckAndMoveBytes()
    {
        if(length<8)//if length==0, it can move;
        {
            moveBytes ( );
        }
        
    }

    private void moveBytes( )
    {
        Array.Copy ( bytes, readIdx, bytes, 0, length );
        writeIdx = length;
        readIdx = 0;
    }

    public int Write( byte[] bs,int offset,int count)
    {
        if (remain < count)
            ReSize ( length + count );
        Array.Copy (  bs, offset, bytes, readIdx, count );
        writeIdx += count;
        return count;
    }
    public int Read( byte[] bs, int offset, int count )
    {
        count = Math.Min ( length, count );
        Array.Copy(bytes,0,bs,offset,count);
        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }
    public Int16 ReadInt16( )
    {
        if(length<2)return 0;
        Int16 ret = (Int16)((bytes[1] << 8) | bytes[0]);
        readIdx += 2;
        CheckAndMoveBytes ();
        return ret;
    }
    public Int32 ReadInt32( )
    {
        if (length < 4) return 0;
        Int16 ret = (Int16)((bytes[3] << 24) |( bytes[2] << 16) |( bytes[1] << 8) | bytes[0]);
        readIdx += 4;
        CheckAndMoveBytes ( );
        return ret;
    }
}