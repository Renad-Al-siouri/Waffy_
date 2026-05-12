using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class PetRepository : IPetRepository
    {
        private readonly string _connectionString;

        public PetRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ════════════════════════════════════════════════════
        // PET CRUD
        // ════════════════════════════════════════════════════

        public async Task<IEnumerable<Pet>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM Pet
                WHERE IdUsers = @UserId
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Pet>(sql, new { UserId = userId });
        }

        public async Task<Pet?> GetByIdAsync(int petId)
        {
            const string sql = "SELECT * FROM Pet WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Pet>(sql, new { IdPet = petId });
        }

        public async Task<int> AddAsync(Pet pet)
        {
            const string sql = @"
                INSERT INTO Pet
                    (IdUsers, Name, Species, Breed, Gender, BirthDate, Color,
                     MicrochipId, Weight, VaccinationStatus, IsNeutered,
                     MedicalNotes, Traits, EnergyLevel, GoodWithChildren,
                     GoodWithPets, IsTrained, ImageUrl, CreatedAt)
                VALUES
                    (@IdUsers, @Name, @Species, @Breed, @Gender, @BirthDate, @Color,
                     @MicrochipId, @Weight, @VaccinationStatus, @IsNeutered,
                     @MedicalNotes, @Traits, @EnergyLevel, @GoodWithChildren,
                     @GoodWithPets, @IsTrained, @ImageUrl, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, pet);
        }

        public async Task UpdateAsync(Pet pet)
        {
            const string sql = @"
                UPDATE Pet SET
                    Name              = @Name,
                    Species           = @Species,
                    Breed             = @Breed,
                    Gender            = @Gender,
                    BirthDate         = @BirthDate,
                    Color             = @Color,
                    MicrochipId       = @MicrochipId,
                    Weight            = @Weight,
                    VaccinationStatus = @VaccinationStatus,
                    IsNeutered        = @IsNeutered,
                    MedicalNotes      = @MedicalNotes,
                    Traits            = @Traits,
                    EnergyLevel       = @EnergyLevel,
                    GoodWithChildren  = @GoodWithChildren,
                    GoodWithPets      = @GoodWithPets,
                    IsTrained         = @IsTrained,
                    ImageUrl          = @ImageUrl
                WHERE IdPet = @IdPet";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, pet);
        }

        public async Task DeleteAsync(int petId)
        {
            // PetImage يُحذف تلقائياً بسبب ON DELETE CASCADE في الجدول
            const string sql = "DELETE FROM Pet WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPet = petId });
        }

        public async Task UpdateImageAsync(int petId, string imageUrl)
        {
            const string sql = "UPDATE Pet SET ImageUrl = @ImageUrl WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPet = petId, ImageUrl = imageUrl });
        }

        public async Task TransferOwnershipAsync(int petId, int newOwnerId)
        {
            const string sql = "UPDATE Pet SET IdUsers = @NewOwnerId WHERE IdPet = @IdPet";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdPet = petId, NewOwnerId = newOwnerId });
        }

        // ════════════════════════════════════════════════════
        // PET IMAGE
        // ════════════════════════════════════════════════════

        /// <summary>
        /// كل صور الحيوان — الرئيسية أولاً ثم حسب SortOrder
        /// </summary>
        public async Task<IEnumerable<PetImage>> GetImagesAsync(int petId)
        {
            const string sql = @"
                SELECT * FROM PetImage
                WHERE  IdPet = @IdPet
                ORDER BY IsMain DESC, SortOrder ASC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<PetImage>(sql, new { IdPet = petId });
        }

        /// <summary>
        /// إضافة صورة جديدة للحيوان.
        /// — لو أول صورة تُعيَّن Main تلقائياً.
        /// — Pet.ImageUrl يُحدَّث لو أصبحت Main.
        /// </summary>
        public async Task<int> AddPetImageAsync(int petId, string imageUrl, bool isMain = false)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // لو لا توجد صور → هذه الصورة تصبح Main تلقائياً
            var existingCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM PetImage WHERE IdPet = @IdPet",
                new { IdPet = petId });

            if (existingCount == 0) isMain = true;

            // لو ستكون Main → أزل IsMain عن الأخريات أولاً
            if (isMain)
            {
                await conn.ExecuteAsync(
                    "UPDATE PetImage SET IsMain = 0 WHERE IdPet = @IdPet",
                    new { IdPet = petId });
            }

            // أضف الصورة الجديدة
            const string insertSql = @"
                INSERT INTO PetImage (IdPet, ImageUrl, SortOrder, IsMain, UploadedAt)
                VALUES (
                    @IdPet,
                    @ImageUrl,
                    (SELECT ISNULL(MAX(SortOrder), -1) + 1 FROM PetImage WHERE IdPet = @IdPet),
                    @IsMain,
                    GETUTCDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var newId = await conn.ExecuteScalarAsync<int>(insertSql,
                new { IdPet = petId, ImageUrl = imageUrl, IsMain = isMain });

            // حدّث Pet.ImageUrl لو هذه الصورة Main
            if (isMain)
            {
                await conn.ExecuteAsync(
                    "UPDATE Pet SET ImageUrl = @ImageUrl WHERE IdPet = @IdPet",
                    new { IdPet = petId, ImageUrl = imageUrl });
            }

            return newId;
        }

        /// <summary>
        /// حذف صورة واحدة ثم إعادة مزامنة Pet.ImageUrl
        /// </summary>
        public async Task DeletePetImageAsync(int imageId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // اجلب بيانات الصورة قبل الحذف (نحتاج IdPet)
            var image = await conn.QueryFirstOrDefaultAsync<PetImage>(
                "SELECT * FROM PetImage WHERE Id = @Id",
                new { Id = imageId });

            if (image == null) return;

            await conn.ExecuteAsync(
                "DELETE FROM PetImage WHERE Id = @Id",
                new { Id = imageId });

            // أعد المزامنة بعد الحذف
            await SyncMainImageInternalAsync(conn, image.IdPet);
        }

        /// <summary>
        /// تعيين صورة محددة كـ Main وتحديث Pet.ImageUrl
        /// </summary>
        public async Task SetMainImageAsync(int petId, int imageId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // اجلب ImageUrl للصورة المحددة أولاً
            var imageUrl = await conn.ExecuteScalarAsync<string?>(
                "SELECT ImageUrl FROM PetImage WHERE Id = @Id AND IdPet = @IdPet",
                new { Id = imageId, IdPet = petId });

            if (imageUrl == null) return; // الصورة غير موجودة أو لا تنتمي للحيوان

            // أزل IsMain عن الجميع
            await conn.ExecuteAsync(
                "UPDATE PetImage SET IsMain = 0 WHERE IdPet = @IdPet",
                new { IdPet = petId });

            // عيّن IsMain=1 على الصورة المطلوبة بالـ Id المحدد
            await conn.ExecuteAsync(
                "UPDATE PetImage SET IsMain = 1 WHERE Id = @Id",
                new { Id = imageId });

            // حدّث Pet.ImageUrl بنفس الـ URL
            await conn.ExecuteAsync(
                "UPDATE Pet SET ImageUrl = @Url WHERE IdPet = @IdPet",
                new { Url = imageUrl, IdPet = petId });
        }

        /// <summary>
        /// مزامنة Pet.ImageUrl مع الصورة الرئيسية في PetImage.
        /// يُستدعى من خارج الـ Repository عند الحاجة.
        /// لو لا توجد صور → Pet.ImageUrl = NULL
        /// </summary>
        public async Task SyncMainImageAsync(int petId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await SyncMainImageInternalAsync(conn, petId);
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPER — يعمل على connection مفتوحة مسبقاً
        // ════════════════════════════════════════════════════

        /// <summary>
        /// الخوارزمية الصحيحة التي تضمن التطابق الكامل:
        ///
        ///  1. اجلب الصورة الأفضل بـ Id و ImageUrl معاً في نفس الـ Query
        ///  2. أزل IsMain عن الجميع
        ///  3. عيّن IsMain=1 على نفس الصورة بالـ Id الذي جلبناه في الخطوة 1
        ///  4. حدّث Pet.ImageUrl بالـ ImageUrl الذي جلبناه في الخطوة 1
        ///
        ///  هكذا Id و ImageUrl مصدرهما نفس الـ Row ولا يوجد تضارب.
        /// </summary>
        private static async Task SyncMainImageInternalAsync(SqlConnection conn, int petId)
        {
            // الخطوة 1: اجلب الصورة الأفضل — أولوية لـ IsMain=1 ثم أقل SortOrder
            var best = await conn.QueryFirstOrDefaultAsync<PetImage>(
                @"SELECT TOP 1 Id, IdPet, ImageUrl, SortOrder, IsMain, UploadedAt
                  FROM   PetImage
                  WHERE  IdPet = @IdPet
                  ORDER BY IsMain DESC, SortOrder ASC",
                new { IdPet = petId });

            if (best != null)
            {
                // الخطوة 2: أزل IsMain عن الجميع
                await conn.ExecuteAsync(
                    "UPDATE PetImage SET IsMain = 0 WHERE IdPet = @IdPet",
                    new { IdPet = petId });

                // الخطوة 3: عيّن IsMain=1 بالـ Id الذي جلبناه — لا توجد subquery هنا
                await conn.ExecuteAsync(
                    "UPDATE PetImage SET IsMain = 1 WHERE Id = @Id",
                    new { Id = best.Id });

                // الخطوة 4: حدّث Pet.ImageUrl بالـ URL الذي جلبناه في نفس الـ Row
                await conn.ExecuteAsync(
                    "UPDATE Pet SET ImageUrl = @Url WHERE IdPet = @IdPet",
                    new { Url = best.ImageUrl, IdPet = petId });
            }
            else
            {
                // لا توجد صور على الإطلاق → فرّغ Pet.ImageUrl
                await conn.ExecuteAsync(
                    "UPDATE Pet SET ImageUrl = NULL WHERE IdPet = @IdPet",
                    new { IdPet = petId });
            }
        }
    }
}