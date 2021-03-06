﻿namespace DSInternals.DataStore
{
    using DSInternals.Common;
    using System;
    using System.Security.AccessControl;
    using Microsoft.Database.Isam;
    using System.Security.Cryptography;

    public class SecurityDescriptorRersolver : IDisposable
    {
        private const string SdValueCol = "sd_value";
        private const string SdHashCol = "sd_hash";
        private const string SdIndex = "sd_id_index";

        private Cursor cursor;

        public SecurityDescriptorRersolver(IsamDatabase database)
        {
            this.cursor = database.OpenCursor(ADConstants.SecurityDescriptorTableName);
            this.cursor.SetCurrentIndex(SdIndex);
        }

        public CommonSecurityDescriptor GetDescriptor(int id)
        {
            bool found = this.cursor.GotoKey(Key.Compose(id));
            if (!found)
            {
                // TODO: Exception type
                throw new Exception();
            }
            var binaryForm = (byte[])this.cursor.Record[SdValueCol];
            return new CommonSecurityDescriptor(true, true, binaryForm, 0);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static byte[] ComputeHash(CommonSecurityDescriptor securityDescriptor)
        {
            if(securityDescriptor == null)
            {
                return null;
            }
            // Convert to binary form
            byte[] binaryForm = new byte[securityDescriptor.BinaryLength];
            securityDescriptor.GetBinaryForm(binaryForm, 0);
            using(var md5 = MD5CryptoServiceProvider.Create())
            {
                return md5.ComputeHash(binaryForm);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.cursor != null)
            {
                this.cursor.Dispose();
                this.cursor = null;
            }
        }
    }
}
