نظام الصلاحيات والتحقق من الهوية (Authorization & Access Control Architecture)
1. الركائز الأساسية لنظام الـ Authorization المقترح
لكي يكون النظام مرناً وقابلاً للتوسع (Scalable)، سنعتمد مزيجاً قوياً من الأنماط المعمارية الأمنية:

Role-Based Access Control (RBAC): لتحديد الأدوار العامة للمستخدمين (مثل: Admin, Auditor, RegularUser, BankManager).

Permission-Based / Claim-Based Authorization: وهي الأسلوب الأكثر احترافية للتحكم الدقيق (Fine-Grained Access Control)، حيث لا نتحقق من الدور فحسب، بل من الصلاحية المباشرة (مثل: Permissions.Users.Create, Permissions.Transactions.Audit).

Policy-Based Authorization: في ASP.NET Core، سنقوم ببناء سياسات أمان مخصصة (Custom Policies) تفحص مطالبات الـ Claims أو شروط النطاق (Domain Rules).

2. البنية المقترحة داخل المشروع (Folder Structure)
ضمن هيكلية الطبقات لديك، يمكن تنظيم جزء الـ Authorization بالشكل التالي:

طبقة النطاق (Domain Layer):

تعريف ثوابت الصلاحيات (Permissions Constants): لتجنب الأخطاء الإملائية (Magic Strings).

ربط المستخدم بأدواره وصلاحياته عبر علاقات DDD سليمة (Entities مثل Role, Permission, UserRole).

طبقة البنية التحتية (Infrastructure Layer):

Custom Claims Principal Factory: لتضمين الأدوار والصلاحيات تلقائياً داخل الـ JWT Claims عند تسجيل الدخول أو التجديد.

Permission Authorization Handler & Requirement: للتحقق من امتلاك المستخدم للصلاحية المطلوبة عبر الـ HttpContext.

طبقة التطبيق / الـ API (Presentation / Application Layer):

[Authorize(Policy = "Permission:Users.Read")] أو استخدام فلترات مخصصة لحماية الـ Endpoints.
3. خطة العمل التنفيذية (Step-by-Step Implementation Plan)
المرحلة الأولى: هندسة نموذج البيانات (Domain Modeling)

إنشاء كيانات Role و Permission وربطها بالمستخدمين (User-Role-Permission Mapping).

تعريف ملف ثابت يحتوي على جميع صلاحيات النظام (Permissions Registry) لتوثيقها مركزياً.

المرحلة الثانية: حقن الصلاحيات في الـ JWT Token

تعديل خدمة توليد الـ Tokens (JwtTokenGenerator) لتضمين صلاحيات وأدوار المستخدم كـ Claims ضمن الـ Access Token لضمان الأداء السريع وعدم استعلام قاعدة البيانات مع كل طلب HTTP.

المرحلة الثالثة: إعداد الـ Policies في الـ Pipeline (Program.cs)

تسجيل الـ Authorization Policies وربطها بـ IAuthorizationHandler مخصص يفحص الـ Claims القادمة من الـ Token.

المرحلة الرابعة: حماية الـ Endpoints وكتابة الاختبارات

تطبيق سمات التحقق ([Authorize]) على الـ Controllers أو الـ Minimal APIs.

كتابة اختبارات التكامل (Integration Tests) للتأكد من أن المستخدم غير الصالح يمنع من الوصول (403 Forbidden) والمستخدم غير المسجل يُرد بـ (401 Unauthorized).