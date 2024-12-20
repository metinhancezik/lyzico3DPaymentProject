﻿using EntityLayer.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccesLayer.Abstract
{
    public interface IOrderInformations : IGenericDal<OrderInformations>
    {
        Task<OrderInformations> GetLastOrderByUserId(long id);
    }
}
