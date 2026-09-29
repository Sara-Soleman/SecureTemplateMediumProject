Common.Domain
Overview
Common.Domain is a robust foundational library built for .NET applications following Domain-Driven Design (DDD) principles. It provides a shared set of reusable domain primitives, base classes, strongly-typed identifiers, structured error handling, and fluent validation guards.

Project Structure
The project is organized into the following key directories:

Errors/: Contains domain error definitions (IDomainError, DomainError) and categorization smart enums (ErrorType).

Events/: Houses domain event contracts, base records, handlers, and metadata attributes (IDomainEvent, DomainEvent, IDomainEventHandler).

Exceptions/: Custom domain exceptions for handling validation failures and eventual consistency anomalies.

Services/: Time abstraction utilities (IDateTimeProvider) for testable time management.

Root Files: Core domain building blocks including entities, aggregate roots, strongly-typed IDs, and value objects.

Core Components
Entities & Aggregates: Base classes (Entity<TModel>, AggregateRoot<TModel>) featuring automatic audit tracking (IAuditableEntity) and domain event management (IAggregateRoot).

Strongly-Typed IDs: Type-safe record wrappers (Id<TModel>, IId) that eliminate primitive obsession and prevent accidental ID mix-ups.

Value Objects: Immutable objects like DateRange defined entirely by their structural values rather than unique database identifiers.

Defensive Validators: A rich extension method library (Validators) providing fail-fast guards with automatic argument expression naming.

Error & Event Handling
Domain Errors: Structured failure models categorized via smart enums (ErrorType) for consistent API and business rule feedback.

Domain Events: Decoupled messaging infrastructure integrated with MediatR (INotificationHandler) to propagate state changes asynchronously across the system.

ما هي طبقة Common.Domain؟
هي القلب النابض للقواعد التجارية (Business Rules) داخل نظامك. على عكس Common.Core التي تحتوي على عقود عامة جداً، فإن Common.Domain تهتم بفهم "كيف يعمل الـ Domain الخاص بك" (مثل الكيانات، التكتلات، وقواعد العمل الصارمة التي تحكمها).

🔍 ماذا تفعل وماذا تحتوي؟
تحتوي على الكيانات المجردة وقواعد التكتل (Entities & Aggregate Roots):

تضم القواعد الأساسية التي تضمن صحة البيانات وتماسكها داخل الموديول (مثل ضمان أن أي كيان رئيسي يمكنه تتبع وتوليد الأحداث الخاصة به).

تدير أحداث الدومين الداخلية (IDomainEvent & Domain Events):

ماذا تفعل؟ عندما يحدث تغيير مهم داخل التطبيق (مثل: تغيير كلمة المرور، أو تحديث بيانات مستخدم)، يقوم الـ Aggregate بتوليد حدث دومين داخلي عبر واجهة IDomainEvent.

إلى أين ترسلها؟ هذه الأحداث لا تخرج خارج الخدمة أو الموديول (على عكس Integration Events)، بل يتم التقاطها محلياً داخل نفس النظام بواسطة الـ DomainEventDispatcher لمعالجة المهام المرتبطة بها (مثل إرسال إشعار داخلي أو تحديث جدول مرتبط) قبل حفظ التغييرات في قاعدة البيانات.

🌱 هل هي ثابتة أم قابلة للتعديل مستقبلاً مع المشاريع الجديدة؟
نعم، قابلة للتعديل والتوسعة بالتأكيد:
عند إضافة موديول جديد، قد تحتاجي إلى إضافة قواعد مشتركة تخص الكيانات (مثل إضافة خصائص عامة للـ Audit مثل CreatedBy و CreatedAt في قاعدة الكيانات، أو إضافة واجهات جديدة لـ Value Objects المشتركة).

طبيعة التعديل: التعديلات هنا تكون إضافية وهيكلية لتوحيد مفهوم الـ Domain عبر موديولاتك المختلفة، وتظل محتفظة باستقلاليتها وعدم ارتباطها بقواعد بيانات معينة أو تقنيات خارجية.