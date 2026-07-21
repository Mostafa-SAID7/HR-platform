# Flutter Implementation Guide
## Setup, Patterns, and Performance Optimization

Complete guide to implementing HR Analytics mobile app with Flutter.

---

## Project Setup

### Prerequisites

```bash
# Install Flutter (3.16+)
# Download from https://flutter.dev/docs/get-started/install

# Verify installation
flutter doctor

# Output should show:
# ✓ Flutter (Channel master)
# ✓ Android toolchain - develop for Android
# ✓ Xcode - develop for iOS
# ✓ Dart
```

### Create Flutter Project

```bash
# Create project
flutter create --org com.hranalytics hr_analytics

cd hr_analytics

# Add platforms
flutter create --platforms=android,ios,web .
```

### pubspec.yaml Dependencies

```yaml
name: hr_analytics
description: HR Analytics Platform Mobile App

environment:
  sdk: '>=3.0.0 <4.0.0'
  flutter: '>=3.16.0'

dependencies:
  flutter:
    sdk: flutter

  # State Management
  flutter_riverpod: ^2.4.0
  riverpod_annotation: ^2.3.0

  # Navigation
  go_router: ^12.0.0

  # HTTP & Serialization
  dio: ^5.3.0
  dio_smart_retry: ^5.4.0
  retrofit: ^4.1.0
  json_annotation: ^4.8.0

  # Local Database
  sqflite: ^2.3.0
  sqlite3_flutter_libs: ^0.5.0
  path_provider: ^2.1.0

  # Cache
  hive: ^2.2.0
  hive_flutter: ^1.1.0

  # UI
  flutter_svg: ^2.0.0
  shimmer: ^3.0.0
  animations: ^2.0.0

  # Firebase
  firebase_core: ^24.2.0
  firebase_messaging: ^14.7.0
  firebase_analytics: ^10.8.0

  # Biometric Authentication
  local_auth: ^2.2.0

  # Secure Storage
  flutter_secure_storage: ^9.0.0

  # Connectivity
  connectivity_plus: ^5.1.0

  # Geolocation (for attendance)
  geolocator: ^9.0.0

  # Charts
  fl_chart: ^0.65.0

  # PDF Export
  pdf: ^3.10.0
  printing: ^5.11.0

  # Utilities
  intl: ^0.19.0
  uuid: ^4.0.0
  equatable: ^2.0.0
  freezed_annotation: ^2.4.0

dev_dependencies:
  flutter_test:
    sdk: flutter

  # Code generation
  build_runner: ^2.4.0
  retrofit_generator: ^7.1.0
  json_serializable: ^6.7.0
  freezed: ^2.4.0
  riverpod_generator: ^2.3.0

  # Testing
  mocktail: ^1.0.0
  integration_test:
    sdk: flutter

  # Linting
  flutter_lints: ^3.0.0
```

---

## Project Structure & Patterns

### Feature-Driven Structure

```
lib/
├── main.dart                              # Entry point
├── config/
│   ├── app_config.dart
│   ├── theme/
│   │   ├── app_colors.dart
│   │   ├── app_text_styles.dart
│   │   └── app_theme.dart
│   └── routes/
│       ├── app_routes.dart
│       └── go_router_provider.dart
│
├── core/
│   ├── models/
│   │   ├── app_exception.dart
│   │   ├── api_response.dart
│   │   └── base_model.dart
│   ├── services/
│   │   ├── connectivity_service.dart
│   │   ├── local_database_service.dart
│   │   ├── secure_storage_service.dart
│   │   └── sync_service.dart
│   └── utils/
│       ├── constants.dart
│       ├── extensions.dart
│       └── validators.dart
│
├── features/
│   ├── auth/
│   │   ├── models/
│   │   │   ├── auth_state.dart
│   │   │   └── user.dart
│   │   ├── repositories/
│   │   │   └── auth_repository.dart
│   │   ├── providers/
│   │   │   └── auth_provider.dart
│   │   └── screens/
│   │       ├── login_screen.dart
│   │       └── splash_screen.dart
│   │
│   ├── employee/
│   │   ├── models/
│   │   │   ├── employee.dart
│   │   │   └── employee_filter.dart
│   │   ├── repositories/
│   │   │   └── employee_repository.dart
│   │   ├── providers/
│   │   │   ├── employee_list_provider.dart
│   │   │   └── employee_detail_provider.dart
│   │   └── screens/
│   │       ├── employee_list_screen.dart
│   │       ├── employee_detail_screen.dart
│   │       └── employee_form_screen.dart
│   │
│   ├── payroll/
│   ├── attendance/
│   ├── performance/
│   └── analytics/
│
└── shared/
    ├── widgets/
    │   ├── app_bar.dart
    │   ├── custom_button.dart
    │   ├── custom_text_field.dart
    │   ├── error_widget.dart
    │   ├── loading_widget.dart
    │   └── error_dialog.dart
    └── providers/
        ├── app_config_provider.dart
        └── http_client_provider.dart
```

---

## Core Implementation Patterns

### 1. Model with Freezed (Immutable)

```dart
// models/employee.dart
import 'package:freezed_annotation/freezed_annotation.dart';

part 'employee.freezed.dart';
part 'employee.g.dart';

@freezed
class Employee with _$Employee {
  const factory Employee({
    required String id,
    required String firstName,
    required String lastName,
    required String email,
    required String department,
    required String jobTitle,
    required DateTime hireDate,
    required double baseSalary,
    required String status,
    required int companyId,
    required DateTime createdAt,
    required DateTime updatedAt,
  }) = _Employee;

  factory Employee.fromJson(Map<String, dynamic> json) =>
      _$EmployeeFromJson(json);
}

// Usage
final employee = Employee(
  id: '123',
  firstName: 'John',
  lastName: 'Doe',
  // ... other fields
);

// Copy with changes (immutable)
final updated = employee.copyWith(
  firstName: 'Jane',
  baseSalary: 85000,
);
```

### 2. Exception Handling

```dart
// models/app_exception.dart
abstract class AppException implements Exception {
  final String message;
  final String? code;
  final dynamic originalException;

  AppException({
    required this.message,
    this.code,
    this.originalException,
  });

  @override
  String toString() => message;
}

class NetworkException extends AppException {
  NetworkException({required String message})
      : super(message: 'Network Error: $message', code: 'NETWORK_ERROR');
}

class UnauthorizedException extends AppException {
  UnauthorizedException({required String message})
      : super(message: 'Unauthorized: $message', code: 'UNAUTHORIZED');
}

class ValidationException extends AppException {
  ValidationException({required String message})
      : super(message: 'Validation Error: $message', code: 'VALIDATION_ERROR');
}

class ServerException extends AppException {
  ServerException({required String message, String? code})
      : super(message: 'Server Error: $message', code: code);
}
```

### 3. API Client with Interceptors

```dart
// services/api/api_client.dart
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final httpClientProvider = Provider<Dio>((ref) {
  final dio = Dio(BaseOptions(
    baseUrl: 'https://api.hranalytics.com',
    connectTimeout: const Duration(seconds: 10),
    receiveTimeout: const Duration(seconds: 10),
    contentType: 'application/json',
  ));

  // Add interceptors
  dio.interceptors.add(AuthInterceptor(ref));
  dio.interceptors.add(LoggingInterceptor());
  dio.interceptors.add(RetryInterceptor());

  return dio;
});

class AuthInterceptor extends QueuedInterceptor {
  final Ref _ref;

  AuthInterceptor(this._ref);

  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    try {
      final token = await _ref.read(secureStorageProvider).getToken();
      
      if (token != null) {
        options.headers['Authorization'] = 'Bearer $token';
      }
      
      handler.next(options);
    } catch (e) {
      handler.next(options);
    }
  }

  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      // Token expired - refresh
      try {
        final refreshed = await _ref.read(authProvider.notifier).refreshToken();
        
        if (refreshed) {
          // Retry request
          final options = err.requestOptions;
          final token = await _ref.read(secureStorageProvider).getToken();
          options.headers['Authorization'] = 'Bearer $token';
          
          final response = await _ref.read(httpClientProvider).request(
                options.path,
                options: Options(method: options.method),
                data: options.data,
              );
          
          handler.resolve(response);
          return;
        }
      } catch (e) {
        handler.next(err);
        return;
      }
    }

    handler.next(err);
  }
}

class LoggingInterceptor extends Interceptor {
  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    debugPrint('→ REQUEST: ${options.method} ${options.path}');
    handler.next(options);
  }

  @override
  Future<void> onResponse(Response response, ResponseInterceptorHandler handler) async {
    debugPrint('← RESPONSE: ${response.statusCode} ${response.requestOptions.path}');
    handler.next(response);
  }

  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    debugPrint('✗ ERROR: ${err.message}');
    handler.next(err);
  }
}

class RetryInterceptor extends Interceptor {
  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    if (_shouldRetry(err) && err.requestOptions.extra['_retry'] ?? 0 < 3) {
      final baseDelay = Duration(milliseconds: 100);
      final retryCount = err.requestOptions.extra['_retry'] ?? 0;
      final delay = baseDelay * (1 << retryCount); // Exponential backoff

      await Future.delayed(delay);

      err.requestOptions.extra['_retry'] = retryCount + 1;

      try {
        final response = await _ref.read(httpClientProvider).fetch(
              err.requestOptions,
            );
        return handler.resolve(response);
      } on DioException catch (e) {
        return handler.next(e);
      }
    }

    handler.next(err);
  }

  bool _shouldRetry(DioException err) {
    return err.type == DioExceptionType.connectionTimeout ||
        err.type == DioExceptionType.receiveTimeout ||
        err.response?.statusCode == 503 ||
        err.response?.statusCode == 429;
  }
}
```

### 4. Repository Pattern

```dart
// repositories/employee_repository.dart
abstract class EmployeeRepository {
  Future<List<Employee>> getEmployees(int companyId, {int page = 1, int pageSize = 20});
  Future<Employee> getEmployee(String employeeId);
  Future<Employee> createEmployee(CreateEmployeeRequest request);
  Future<Employee> updateEmployee(String employeeId, UpdateEmployeeRequest request);
  Future<void> deleteEmployee(String employeeId);
}

class EmployeeRepositoryImpl implements EmployeeRepository {
  final Dio _dio;
  final LocalDatabaseService _localDb;
  final ConnectivityService _connectivity;

  EmployeeRepositoryImpl(this._dio, this._localDb, this._connectivity);

  @override
  Future<List<Employee>> getEmployees(int companyId, {int page = 1, int pageSize = 20}) async {
    try {
      // Check if online
      if (await _connectivity.isConnected) {
        final response = await _dio.get('/api/v1/employees', queryParameters: {
          'companyId': companyId,
          'page': page,
          'pageSize': pageSize,
        });

        final employees = (response.data['data'] as List)
            .map((e) => Employee.fromJson(e as Map<String, dynamic>))
            .toList();

        // Cache locally
        await _localDb.saveEmployees(employees);

        return employees;
      } else {
        // Offline - use local cache
        return await _localDb.getEmployees(companyId);
      }
    } on DioException catch (e) {
      if (await _connectivity.isConnected) {
        throw _handleDioException(e);
      } else {
        // Fallback to local cache on network error
        final cached = await _localDb.getEmployees(companyId);
        if (cached.isNotEmpty) return cached;
        throw NetworkException(message: 'No internet connection');
      }
    }
  }

  AppException _handleDioException(DioException err) {
    switch (err.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.receiveTimeout:
        return NetworkException(message: 'Connection timeout');
      case DioExceptionType.badResponse:
        if (err.response?.statusCode == 401) {
          return UnauthorizedException(message: 'Session expired');
        } else if (err.response?.statusCode == 404) {
          return AppException(message: 'Resource not found');
        } else if (err.response?.statusCode == 400) {
          return ValidationException(message: err.response?.data['message'] ?? 'Invalid data');
        }
        return ServerException(message: 'Server error', code: '${err.response?.statusCode}');
      default:
        return NetworkException(message: err.message ?? 'Unknown error');
    }
  }
}
```

---

## Performance Optimization

### 1. Image Caching

```dart
// Precache images on app start
final preCacheImagesProvider = FutureProvider<void>((ref) async {
  final imageProvider = AssetImage('assets/images/logo.png');
  await precacheImage(imageProvider, ref.context);
});

// Use cached images
CachedNetworkImage(
  imageUrl: 'https://api.hranalytics.com/avatar/user123.jpg',
  placeholder: (context, url) => const Shimmer.fromColors(
    baseColor: Colors.grey,
    highlightColor: Colors.white,
    child: SizedBox(width: 48, height: 48),
  ),
  errorWidget: (context, url, error) => const Icon(Icons.error),
  cacheManager: CacheManager.instance,
)
```

### 2. Lazy Loading Lists

```dart
// Use ListView.builder, not ListView with many items
ListView.builder(
  itemCount: employees.length,
  itemBuilder: (context, index) {
    // Only builds visible items
    return EmployeeListTile(employee: employees[index]);
  },
  // Paginated loading
  onEndReached: () async {
    await ref.read(employeeListProvider.notifier).loadNextPage();
  },
)
```

### 3. Optimize Build with Consumer

```dart
// Only rebuild affected widgets
Consumer(
  builder: (context, ref, child) {
    // This only rebuilds when employeeListProvider changes
    final employees = ref.watch(employeeListProvider);
    
    return employees.when(
      data: (data) => EmployeeList(employees: data),
      loading: () => const LoadingWidget(),
      error: (err, _) => ErrorWidget(error: err),
    );
  },
  child: const AppBar(title: Text('Employees')), // Not rebuilt
)
```

### 4. Database Indexing

```dart
// Create indexed columns for fast queries
Future<void> createEmployeeTable(Database db) async {
  await db.execute('''
    CREATE TABLE employees (
      id TEXT PRIMARY KEY,
      company_id INTEGER NOT NULL,
      first_name TEXT NOT NULL,
      last_name TEXT NOT NULL,
      email TEXT NOT NULL,
      department TEXT,
      status TEXT DEFAULT 'ACTIVE',
      created_at TIMESTAMP,
      updated_at TIMESTAMP
    )
  ''');
  
  // Create indexes
  await db.execute('CREATE INDEX idx_company_id ON employees(company_id)');
  await db.execute('CREATE INDEX idx_status ON employees(status)');
  await db.execute('CREATE INDEX idx_department ON employees(department)');
}
```

---

## Testing

### Unit Tests

```dart
// test/repositories/employee_repository_test.dart
import 'package:mocktail/mocktail.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('EmployeeRepository', () {
    late MockDio mockDio;
    late MockLocalDatabase mockDb;
    late EmployeeRepositoryImpl repository;

    setUp(() {
      mockDio = MockDio();
      mockDb = MockLocalDatabase();
      repository = EmployeeRepositoryImpl(mockDio, mockDb);
    });

    test('getEmployees returns list from API', () async {
      // Arrange
      final mockResponse = Response(
        data: {
          'data': [
            {'id': '1', 'firstName': 'John', 'lastName': 'Doe'},
          ]
        },
        statusCode: 200,
        requestOptions: RequestOptions(path: ''),
      );

      when(() => mockDio.get(any(), queryParameters: any(named: 'queryParameters')))
          .thenAnswer((_) async => mockResponse);

      // Act
      final result = await repository.getEmployees(1);

      // Assert
      expect(result, isA<List<Employee>>());
      expect(result.length, 1);
      expect(result[0].id, '1');
    });
  });
}

class MockDio extends Mock implements Dio {}
class MockLocalDatabase extends Mock implements LocalDatabaseService {}
```

---

**Last Updated:** July 2026
**Status:** Flutter Implementation Complete
