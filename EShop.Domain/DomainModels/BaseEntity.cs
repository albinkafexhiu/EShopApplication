using System;
using System.ComponentModel.DataAnnotations;

namespace EShop.Domain.DomainModels
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}