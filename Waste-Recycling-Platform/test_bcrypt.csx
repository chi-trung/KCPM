#r "nuget: BCrypt.Net-Next, 4.0.3"
using System;
string hash = "$2b$11$tN7EUn/GW3UfJFw4OFtpKewSWNBk5wmj8VmJHm.sVFWcL.dpx63PK";
bool result = BCrypt.Net.BCrypt.Verify("password", hash);
Console.WriteLine("Match: " + result);
