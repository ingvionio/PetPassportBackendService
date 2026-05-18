using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PetPassport.Models;

namespace PetPassport.Services
{
    public static class PetPassportPdfGenerator
    {
        public static byte[] Generate(
            Pet pet,
            List<VaccineEvent> vaccines,
            List<TreatmentEvent> treatments,
            List<DoctorVisitEvent> visits,
            bool includeVaccines,
            bool includeTreatments,
            bool includeVisits,
            string wwwRootPath)
        {
            return Document.Create(container =>
            {
                AddPassportPage(container, pet, wwwRootPath);

                if (includeVaccines)
                    AddVaccinesPage(container, vaccines);

                if (includeTreatments)
                    AddTreatmentsPage(container, treatments);

                if (includeVisits)
                    AddVisitsPage(container, visits);

            }).GeneratePdf();
        }

        // ── Страницы ──────────────────────────────────────────────────────────

        private static void AddPassportPage(IDocumentContainer container, Pet pet, string wwwRootPath)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .PaddingBottom(10)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Text("Ветеринарный паспорт")
                    .SemiBold().FontSize(22).AlignCenter();

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Spacing(15);

                    // Фото + идентификация
                    col.Item().Row(row =>
                    {
                        var photoPath = GetPhotoPath(pet, wwwRootPath);
                        if (photoPath != null)
                        {
                            row.ConstantItem(160).Height(160).Image(photoPath).FitArea();
                        }
                        else
                        {
                            row.ConstantItem(160).Height(160)
                                .Background(Colors.Grey.Lighten3)
                                .AlignCenter().AlignMiddle()
                                .Text("Нет фото").FontColor(Colors.Grey.Medium);
                        }

                        row.RelativeItem().PaddingLeft(16).Column(info =>
                        {
                            info.Spacing(7);
                            InfoRow(info, "Имя", pet.Name);
                            InfoRow(info, "Вид", SpeciesName(pet.Species));
                            InfoRow(info, "Порода", pet.Breed);
                            InfoRow(info, "Пол", GenderName(pet.Gender));
                            InfoRow(info, "Окрас", pet.Color);
                            InfoRow(info, "Дата рождения", pet.BirthDate?.ToString("dd.MM.yyyy"));
                            InfoRow(info, "Вес", pet.WeightKg.HasValue ? $"{pet.WeightKg} кг" : null);
                        });
                    });

                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // Медицинская информация
                    col.Item().Column(med =>
                    {
                        med.Item().Text("Медицинская информация").SemiBold().FontSize(14);
                        med.Item().PaddingTop(8).Column(rows =>
                        {
                            rows.Spacing(7);
                            InfoRow(rows, "Номер микрочипа", pet.MicrochipNumber);
                            InfoRow(rows, "Стерилизован / кастрирован",
                                pet.IsNeutered.HasValue ? (pet.IsNeutered.Value ? "Да" : "Нет") : null);
                            InfoRow(rows, "Группа крови", pet.BloodType);
                            InfoRow(rows, "Аллергии", pet.Allergies);
                            InfoRow(rows, "Хронические заболевания", pet.ChronicConditions);
                        });
                    });
                });
            });
        }

        private static void AddVaccinesPage(IDocumentContainer container, List<VaccineEvent> vaccines)
        {
            container.Page(page =>
            {
                SetupTablePage(page, "Прививки");

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);    // Дата
                        cols.RelativeColumn(3);    // Название
                        cols.RelativeColumn(3);    // Препарат
                        cols.RelativeColumn(2.5f); // Следующая дата
                        cols.RelativeColumn(2f);   // Статус
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Дата", "Название", "Препарат", "Следующая дата", "Статус" })
                            h.Cell().Element(HeaderCell).Text(t);
                    });

                    foreach (var v in vaccines.OrderBy(x => x.EventDate))
                    {
                        table.Cell().Element(DataCell).Text(v.EventDate.ToString("dd.MM.yyyy"));
                        table.Cell().Element(DataCell).Text(v.Title);
                        table.Cell().Element(DataCell).Text(v.Medicine);
                        table.Cell().Element(DataCell).Text(v.NextVaccinationDate?.ToString("dd.MM.yyyy") ?? "—");
                        table.Cell().Element(DataCell).Text(StatusName(v.Status));
                    }
                });
            });
        }

        private static void AddTreatmentsPage(IDocumentContainer container, List<TreatmentEvent> treatments)
        {
            container.Page(page =>
            {
                SetupTablePage(page, "Обработки от паразитов");

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2.5f);
                        cols.RelativeColumn(2.5f);
                        cols.RelativeColumn(2.5f);
                        cols.RelativeColumn(2f);
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Дата", "Название", "Средство", "Паразит", "Следующая дата", "Статус" })
                            h.Cell().Element(HeaderCell).Text(t);
                    });

                    foreach (var tr in treatments.OrderBy(x => x.EventDate))
                    {
                        table.Cell().Element(DataCell).Text(tr.EventDate.ToString("dd.MM.yyyy"));
                        table.Cell().Element(DataCell).Text(tr.Title);
                        table.Cell().Element(DataCell).Text(tr.Remedy);
                        table.Cell().Element(DataCell).Text(tr.Parasite);
                        table.Cell().Element(DataCell).Text(tr.NextTreatmentDate?.ToString("dd.MM.yyyy") ?? "—");
                        table.Cell().Element(DataCell).Text(StatusName(tr.Status));
                    }
                });
            });
        }

        private static void AddVisitsPage(IDocumentContainer container, List<DoctorVisitEvent> visits)
        {
            container.Page(page =>
            {
                SetupTablePage(page, "Визиты к врачу");

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2f);
                    });

                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Дата", "Название", "Клиника", "Врач", "Диагноз", "Статус" })
                            h.Cell().Element(HeaderCell).Text(t);
                    });

                    foreach (var v in visits.OrderBy(x => x.EventDate))
                    {
                        table.Cell().Element(DataCell).Text(v.EventDate.ToString("dd.MM.yyyy"));
                        table.Cell().Element(DataCell).Text(v.Title);
                        table.Cell().Element(DataCell).Text(v.Clinic);
                        table.Cell().Element(DataCell).Text(v.Doctor);
                        table.Cell().Element(DataCell).Text(v.Diagnosis ?? "—");
                        table.Cell().Element(DataCell).Text(StatusName(v.Status));
                    }
                });
            });
        }

        // ── Вспомогательные методы ────────────────────────────────────────────

        private static void SetupTablePage(PageDescriptor page, string title)
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10));
            page.Header()
                .PaddingBottom(10)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Text(title)
                .SemiBold().FontSize(18).AlignCenter();
        }

        private static void InfoRow(ColumnDescriptor col, string label, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            col.Item().Row(row =>
            {
                row.ConstantItem(180).Text(label + ":").SemiBold();
                row.RelativeItem().Text(value);
            });
        }

        private static string? GetPhotoPath(Pet pet, string wwwRootPath)
        {
            var first = pet.Photos.FirstOrDefault();
            if (first is null) return null;
            var path = Path.Combine(wwwRootPath, first.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(path) ? path : null;
        }

        private static IContainer HeaderCell(IContainer c) =>
            c.DefaultTextStyle(x => x.SemiBold())
             .Background(Colors.Grey.Lighten3)
             .Border(0.5f)
             .BorderColor(Colors.Grey.Lighten1)
             .Padding(5);

        private static IContainer DataCell(IContainer c) =>
            c.Border(0.5f)
             .BorderColor(Colors.Grey.Lighten2)
             .Padding(4);

        private static string? SpeciesName(PetSpecies? s) => s switch
        {
            PetSpecies.Cat            => "Кот",
            PetSpecies.Rat            => "Крыса",
            PetSpecies.Dog            => "Собака",
            PetSpecies.Lizard         => "Ящерица",
            PetSpecies.Parrot         => "Попугай",
            PetSpecies.Owl            => "Сова",
            PetSpecies.Snail          => "Улитка",
            PetSpecies.Hedgehog       => "Ёж",
            PetSpecies.Butterfly      => "Бабочка",
            PetSpecies.Snake          => "Змея",
            PetSpecies.Frog           => "Лягушка",
            PetSpecies.Rooster        => "Петух",
            PetSpecies.CrabOrCrayfish => "Краб/рак",
            PetSpecies.Fish           => "Рыба",
            PetSpecies.RabbitOrHare   => "Кролик/заяц",
            PetSpecies.Hamster        => "Хомяк",
            _                         => null
        };

        private static string? GenderName(PetGender? g) => g switch
        {
            PetGender.Male   => "Самец",
            PetGender.Female => "Самка",
            _                => null
        };

        private static string StatusName(EventStatus s) => s switch
        {
            EventStatus.Indefinite => "Неопределено",
            EventStatus.Upcoming   => "Предстоит",
            EventStatus.Completed  => "Выполнено",
            EventStatus.Cancelled  => "Отменено",
            _                      => s.ToString()
        };
    }
}
