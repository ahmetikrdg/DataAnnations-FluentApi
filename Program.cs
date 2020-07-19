using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFD
{

    public class ShopContext : DbContext
    {

        public DbSet<Product> products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });//entity komutum sqlde nasıl göremk için ekledim
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(MyLoggerFactory).UseMySql(@"server=localhost;port=3306;database=ShopDb;user=root;password=1532blmz");

            // //entity komutum sqlde nasıl göremk için ekledim bu satırı
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()//customer tablosuna konumlandım
                   .Property(b => b.Id)//property özelliğilye ilgili elemana gidiyoruz
                   .IsRequired();//diyerek zorunlu alan olarak tanımladık

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)//userin hangi alanda ona index ekle
            .IsUnique();//ve bu benzersiz olmalı

            modelBuilder.Entity<ProductCategory>()
                .HasKey(bc => new { bc.ProductId, bc.CategoryId });
            modelBuilder.Entity<ProductCategory>()
                .HasOne(bc => bc.product)
                .WithMany(b => b.ProductCategories)
                .HasForeignKey(bc => bc.ProductId);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(bc => bc.category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(bc => bc.CategoryId);
        }
    }
    public class User
    {//one to many bir kullanıcının birden fazla adresi olabilir
        public int Id { get; set; }
        [Required]
        [MaxLength(15), MinLength(10)]
        public string Username { get; set; }
        public string Email { get; set; }
        public List<Address> Addresses { get; set; }//elde ettiğim herhangi userin üzerinden adresses dediğim zaman adres bilgisi gelecek.bir userin birde nfazla adresi olabileceği için list
                                                    //herhang bir kullanıcı üzerinden addreses dersem o kullanıcının adersi gelir
                                                    //bir kullanıcının birden fazla adresi olabilir. Birkaç tane adres yanlızca bir usere ait olmalı.
    }

    public class Customer
    {
        [Column("Customer_id")]//uygulamadaki Id Customer_id olur 
        public int Id { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]//tabloda gözükmesin
        public string FullName { get; set; }
        public User User { get; set; }//iki tablodada böyle tanımladım list tanımlasaydım userde bire çok olurdu.
        public int UserId { get; set; }//neden userıd customerda
        //bir user kaydını database eklemem gerekir ve bu userin oluşturulan ıd bilgisini alıp o id bilgisiyle customer kaydı oluşturacaksın.Ancak oluşturulan user ıd bilgisini işte bu tabloya eklemen lazım.
    }
    public class Address
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public User User { get; set; }//user objesi oluşturdum.adresler tablosundaki her kayıt tek bir usere ait.bir user birine ait o yüzden user yukarıda list çünkü birden fazla adresi olabilir dedik
        public int UserId { get; set; }//eklemiş olduğum herhangi bir userin ıd bilgisini kullanarak gelip adres tablosuna bir kayırt ekleyebilirim
        //murlaka bir userin ıdsi var demek int? yaparsan nulda olsa sıkıntı olmaz//yabancı anahtar bu user
    }
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]//artık Id kolonu kendi kendine artmaz bizim yazmamız gerekir
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]//kayıt oluştururken kayıt tarihini alır 
        public DateTime InsertedDate { get; set; } = DateTime.Now;
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LastUpdatedDate { get; set; }

        public int CategoryId { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }

    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
    }
    public class ProductCategory
    {
        public int ProductId { get; set; }
        public Product product { get; set; }
        public int CategoryId { get; set; }
        public Category category { get; set; }
    }
    public class Order
    {
        public int Id { get; set; }
        public int ProdutId { get; set; }
        public DateTime DateAdded { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (var DB = new ShopContext())
            {
                var p=new Product(){
                    Name="Samsung s6",
                    Price=2000

                };
                DB.products.Add(p);
                DB.SaveChanges();

            }
        }
        static void InsertUsers()
        {
            var users = new List<User>(){
                new User(){Username="Ahmet Karadağ",Email="ahmetikrdg@outlook.com"},
                new User(){Username="Yiğit Bilge",Email="yigitbilge@outlook.com"},
                new User(){Username="Ali Çelik",Email="celikali@outlook.com"},
                new User(){Username="Mehmet Güz",Email="cmet@outlook.com"}
            };
            using (var db = new ShopContext())
            {
                db.Users.AddRange(users);
                db.SaveChanges();
            }
        }
        static void InsertAddresess()
        {
            var Adreses = new List<Address>(){
                new Address(){Fullname="Ahmet Karadağ",Title="Ev Adresi",Body="İstanbul",UserId=1},
                new Address(){Fullname="Yiğit Bilge",Title="İş Adresi",Body="İstanbul",UserId=2},
                new Address(){Fullname="Ali Çelik",Title="Ev Adresi",Body="İstanbul",UserId=3},
                                new Address(){Fullname="Ali Çelik",Title="İş Adresi",Body="İstanbul",UserId=3},
                                new Address(){Fullname="Mehmet Güz",Title="İş Adresi",Body="İstanbul",UserId=4},
                                new Address(){Fullname="Mehmet Güz",Title="Ev Adresi",Body="İstanbul",UserId=4}
            };
            using (var db = new ShopContext())
            {
                db.Addresses.AddRange(Adreses);
                db.SaveChanges();
            }
        }
    }
    /*
        [NotMapped]//database içerisinde bir tablo olarak oluşturulmaz
        veya tablo ismi şöyle olsun 
        [Table("UrunKategorileri")]
        yada bu tablo adı değiştiremnin fluentli hali
        modelBuilder.Entity<Car>().ToTable("Urunler");
        class Car
        {
            [Key] //LicensePlate id olsun dedim anlaması için key dedim
            public string LicensePlate { get; set; }

            public string Make { get; set; }
            public string Model { get; set; }
            ----------------
             fluent apili hali shopcontextin içinde olmalı.
                protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Car>()
                    .HasKey(c => c.LicensePlate);
            }*/
}








