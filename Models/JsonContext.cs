using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DO NOT REMOVE. Otherwise the program won't load after the config was created 

using System.Text.Json.Serialization;
using mpv_audio.Models;

namespace mpv_audio;

[JsonSerializable(typeof(Cache))]
[JsonSerializable(typeof(Config))]
internal partial class JsonContext : JsonSerializerContext
{
}