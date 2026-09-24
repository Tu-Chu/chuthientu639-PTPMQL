using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace DemoMVC.Models

{
    [Table("Person")]
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        public required string FullName { get; set; }
        public int Address { get; set; }
    }
}
