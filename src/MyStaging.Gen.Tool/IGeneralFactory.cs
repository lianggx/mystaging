using MyStaging.Gen.Tool.Models;
using System;

namespace MyStaging.Gen.Tool
{
    public interface IGeneralFactory
    {
        void Build(ProjectConfig config);
    }
}
