/*
 * Copyright 2015-2021 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using System;
using System.IO;
using EliteDangerousCore.DB;

namespace EliteDangerousCore
{
    // this is bound into the TLU, so its really a class using the TLU, its not a generic log reader, so rename it thus

    public class TravelLogUnitLogReader
    {
        // File buffer
        Stream stream;

        // in memory buffer
        protected byte[] buffer;
        protected int readpos;
        protected int storedlen;

        // File Information
        public TravelLogUnit TravelLogUnit { get; protected set; }
        public string FullName { get { return TravelLogUnit.FullName; } }
        public int Pos { get { return TravelLogUnit.Size; } set { TravelLogUnit.Size = value; } }
        public long ID { get { return TravelLogUnit.ID; } }

        public TravelLogUnitLogReader(string filename)
        {
            this.TravelLogUnit = new TravelLogUnit(filename);
        }

        public TravelLogUnitLogReader(TravelLogUnit tlu)
        {
            this.TravelLogUnit = tlu;
        }

        public string ReadLine()
        {
            if (stream == null)                     // Initialise stream at TLU Pos
            { 
                try
                {
                    stream = File.Open(FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);      // may except
                    stream.Seek(Pos, SeekOrigin.Begin);     // seek to position based on TLU
                    //System.Diagnostics.Debug.WriteLine("Open " + FullName + " at " + Pos);
                }
                catch
                {
                    stream?.Dispose();
                    stream = null;
                    return null;
                };
            }

            if (buffer == null)                     // Initialize buffer if not yet allocated
            {
                readpos = 0;
                storedlen = 0;
                buffer = new byte[16384];
            }

            while (true)        // go around till EOF or valid line
            {
                if (readpos < storedlen)                            // if we have data in buffer
                {
                    // Find the next end-of-line, 0 based index offset
                    int endlineoffset = Array.IndexOf(buffer, (byte)'\n', readpos, storedlen - readpos) - readpos;

                    //System.Diagnostics.Debug.WriteLine("At {0} len {1} eol {2}", readpos, storedlen, endlineoffset);

                    if (endlineoffset >= 0)                         // if we find EOL, process the line, else we will grab some more below
                    {
                        Pos += endlineoffset + 1;                   // tlu moves on past

                        int lineendpos = readpos + endlineoffset + 1; // always past the \n
                        int textendpos = lineendpos - 1;           
                        if ( textendpos>0 && buffer[textendpos-1] == '\r')      // note we could have a \n at the beginning, so we need to make sure we check before text (bug dec 20)
                            textendpos--;

                        while (readpos < textendpos && buffer[readpos] == 0)  // filter out any pre nulls, we have seen these
                            readpos++;

                        int linelen = textendpos - readpos;         // is the difference between them

                        byte[] buf = new byte[linelen];
                        Buffer.BlockCopy(buffer, readpos, buf, 0, linelen);

                        readpos = lineendpos;

                        // we return with a line, with the stream open and buffer occupied.  
                        // next time, when it asks for the next line, we won't have enough, we won't read anything and the
                        // file will be closed and buffer deallocated.  Any bits of half line if present will be lost, but
                        // when we try again, we will reopen, at Pos, read the line and go again
                        // this allows us to read multiple lines without doing a constant reopen.

                      //  System.Diagnostics.Debug.WriteLine("Line len " +linelen  + " Pos now " + Pos);

                        return System.Text.Encoding.UTF8.GetString(buf);
                    }
                }

                // No end-of-line found in buffer or no data in buffer, so get some more.

                // Move remaining data to start of buffer
                if (readpos != 0)
                {
                    //System.Diagnostics.Debug.WriteLine("Slide back buffer from {0} to 0 storing {1}", readpos, storedlen - readpos);
                    Buffer.BlockCopy(buffer, readpos, buffer, 0, storedlen - readpos);
                    storedlen -= readpos;
                    //System.Diagnostics.Debug.WriteLine(".. leaving {0} stored", storedlen);
                    readpos = 0;
                }

                // Expand the buffer if buffer is full but no NL was found
                // this can occur because the line length is longer than the current buffer.Length size
                if (storedlen == buffer.Length)
                {
                    Array.Resize(ref buffer, buffer.Length * 2);
                    //System.Diagnostics.Debug.WriteLine("Resize" + buffer.Length);
                }

                try
                {
                    //System.Diagnostics.Debug.WriteLine("Read to {0} len {1}", storedlen, buffer.Length-storedlen);
                    int bytesread = stream.Read(buffer, storedlen, buffer.Length - storedlen);

                    if ( bytesread == 0 )           // we are here because there is no data left or no LF found, so if we did not read anything, we are over
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                        buffer = null;
                        //System.Diagnostics.Debug.WriteLine("Close no more data " + FullName + " at " + Pos);
                        return null;
                    }

                    storedlen += bytesread;
                    //System.Diagnostics.Debug.WriteLine(".. leaving {0} stored", storedlen);
                }
                catch (Exception ex)        // OS threw exception, abort
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                    buffer = null;
                    System.Diagnostics.Trace.WriteLine($"Error reading journal {FullName}: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
