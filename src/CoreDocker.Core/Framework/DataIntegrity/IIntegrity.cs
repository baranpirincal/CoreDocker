﻿using System;
using System.Threading.Tasks;
using CoreDocker.Dal.Persistance;

namespace CoreDocker.Core.Framework.DataIntegrity
{
    public interface IIntegrity
    {
        bool UpdateAllowed<T>(T updatedValue);
        Task<long> UpdateReferences<T>(IGeneralUnitOfWork generalUnitOfWork, T updatedValue);
        Task<long> GetReferenceCount<T>(IGeneralUnitOfWork generalUnitOfWork, T updatedValue);
        Task<bool> HasReferenceChanged<T>(IGeneralUnitOfWork generalUnitOfWork, T updatedValue);
        bool IsIntegration(Type dalType, string property);
    }
}