using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace testCVD
{

    #region Определяем сущности для работы с таблицами 
    [Table(Name = "employee")]
    public class employee
    {
        [Column(IsPrimaryKey = false, IsDbGenerated = false)]
        public int id { get; set; }
        [Column]
        public int department_id { get; set; }
        [Column]
        public int chief_id { get; set; }
        [Column]
        public string Name { get; set; }
        [Column]
        public int salary { get; set; }

    }

    [Table(Name = "department")]
    public class department
    {
        [Column(IsPrimaryKey = false, IsDbGenerated = false)]
        public int id { get; set; }
        [Column]
        public string Name { get; set; }

    }
    #endregion


    class Program
    {
        static string connectionString = @"Data Source=s-main-db02;Initial Catalog=TestDB;Integrated Security=True";
        static void Main(string[] args)
        {
            DataContext db = new DataContext(connectionString);
            //получаем таблицы
            Table<employee> employee = db.GetTable<employee>();
            Table<department> department = db.GetTable<department>();

            #region Суммарную зарплату в разрезе департаментов с руководителями
            var query = department.GroupJoin(employee, 
                                        d => d.id, 
                                        e => e.department_id,
                                        (dep, empl) => new
                                        {
                                            DepName = dep.Name,
                                            SumSalary = empl.Sum(s => s.salary)
                                        });
            Console.WriteLine("Суммарная зарплата в разрезе департаментов с руководителями:");
            foreach (var r in query)
            {
                Console.WriteLine("Департамент: {0}", r.DepName);
                Console.WriteLine("Сумма: {0}", r.SumSalary);
            }
            #endregion

            #region Суммарную зарплату в разрезе департаментов без руководителей
            Console.WriteLine("\nСуммарная зарплата в разрезе департаментов без руководителей:");
            var query1 = from em in employee
                         join d in department on em.department_id equals d.id
                         //Подзапросом убираем руководителей
                         where !(from r in employee
                                 select r.chief_id).Contains(em.id)
                         group em by d.Name
                         into ResTable
                         select new { NameDep = ResTable.Key, SumSalary = ResTable.Sum(s => s.salary) };

            foreach (var q in query1)
            {
                Console.WriteLine("Подразделение: {0}", q.NameDep);
                Console.WriteLine("Сумма: {0}", q.SumSalary);
            }
            #endregion

            #region Департамент, в котором у сотрудника зарплата максимальна
            Console.WriteLine("\nДепартамент, в котором у сотрудника зарплата максимальна:");
            var query2 = (from d in department
                        join e in employee on d.id equals e.department_id
                        orderby e.salary descending
                        select d.Name).FirstOrDefault();
            Console.WriteLine(query2);
            #endregion

            #region Зарплаты руководителей департаментов (по убыванию)
            Console.WriteLine("\nЗарплаты руководителей департаментов (по убыванию):");
            var query3 = (from e in employee
                        join r in employee on e.chief_id equals r.id
                        group e by new { r.Name, r.salary }
                        into ResTable
                        orderby ResTable.Key.salary descending
                        select ResTable);

            foreach (var q in query3)
            {
                Console.WriteLine("Руководитель: {0}", q.Key.Name);
                Console.WriteLine("Сумма: {0}", q.Key.salary);
            }
            #endregion

            Console.ReadKey();
        }
    }
}
