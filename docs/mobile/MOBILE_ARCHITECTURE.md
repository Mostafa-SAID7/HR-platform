# Mobile Architecture
## State Management, Navigation, API Integration

Comprehensive architectural patterns for Flutter-based HR Analytics mobile app.

---

## Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────────┐
│  Presentation Layer (Screens/Widgets)   │
│  • Screens                              │
│  • Widgets                              │
│  • Theme management                     │
└─────────────────────────────────────────┘
           │ Uses (Providers)
           ▼
┌─────────────────────────────────────────┐
│  State Management Layer (Riverpod)      │
│  • Providers (State holders)            │
│  • Async providers                      │
│  • Notifiers                            │
└─────────────────────────────────────────┘
           │ Calls
           ▼
┌─────────────────────────────────────────┐
│  Domain Layer (Use Cases/Models)        │
│  • DTOs                                 │
│  • Models                               │
│  • Validators                           │
└─────────────────────────────────────────┘
           │ Uses
           ▼
┌─────────────────────────────────────────┐
│  Data Layer (Repositories)              │
│  • API Clients                          │
│  • Local Database                       │
│  • Cache                                │
└─────────────────────────────────────────┘
           │
      ┌────┴────┐
      ▼         ▼
   Backend   Local DB
   (gRPC)    (SQLite)
```

---

## Project Structure

```
hr_analytics_mobile/
├── analysis_options.yaml                 # Linting rules
├── pubspec.yaml                          # Dependencies
├── lib/
│   ├── main.dart                         # Entry point
│   ├── config/
│   │   ├── app_config.dart              # Environment config
│   │   ├── theme/
│   │   │   ├── app_theme.dart           # Material Design 3
│   │   │   └── colors.dart              # Color palette
│   │   └── routes/
│   │       ├── app_routes.dart          # Route definitions
│   │       └── route_observer.dart      # Navigation tracking
│   │
│   ├── models/
│   │   ├── employee.dart
│   │   ├── payroll.dart
│   │   ├── attendance.dart
│   │   ├── performance.dart
│   │   └── exceptions.dart
│   │
│   ├── providers/                        # Riverpod providers (State)
│   │   ├── auth_provider.dart
│   │   ├── employee_provider.dart
│   │   ├── payroll_provider.dart
│   │   ├── attendance_provider.dart
│   │   ├── connectivity_provider.dart
│   │   └── cache_provider.dart
│   │
│   ├── services/                         # Business logic
│   │   ├── api/
│   │   │   ├── api_client.dart
│   │   │   ├── employee_api.dart
│   │   │   ├── payroll_api.dart
│   │   │   └── attendance_api.dart
│   │   ├── local/
│   │   │   ├── local_database.dart
│   │   │   └── secure_storage.dart
│   │   └── sync/
│   │       └── sync_service.dart        # Offline sync
│   │
│   ├── repositories/                     # Data access layer
│   │   ├── employee_repository.dart
│   │   ├── payroll_repository.dart
│   │   └── attendance_repository.dart
│   │
│   ├── screens/                          # UI Layer
│   │   ├── auth/
│   │   │   ├── login_screen.dart
│   │   │   └── splash_screen.dart
│   │   ├── employees/
│   │   │   ├── employee_list_screen.dart
│   │   │   ├── employee_detail_screen.dart
│   │   │   └── employee_form_screen.dart
│   │   ├── payroll/
│   │   │   ├── payroll_list_screen.dart
│   │   │   ├── payslip_screen.dart
│   │   │   └── payroll_detail_screen.dart
│   │   ├── attendance/
│   │   │   ├── checkin_screen.dart
│   │   │   ├── attendance_history_screen.dart
│   │   │   └── leave_request_screen.dart
│   │   ├── analytics/
│   │   │   ├── dashboard_screen.dart
│   │   │   ├── reports_screen.dart
│   │   │   └── chart_screen.dart
│   │   └── common/
│   │       ├── error_screen.dart
│   │       └── loading_screen.dart
│   │
│   ├── widgets/                          # Reusable components
│   │   ├── app_bar.dart
│   │   ├── card_widgets.dart
│   │   ├── buttons.dart
│   │   ├── form_fields.dart
│   │   ├── charts.dart
│   │   └── dialogs.dart
│   │
│   └── utils/
│       ├── constants.dart
│       ├── extensions.dart
│       ├── formatters.dart
│       └── validators.dart
│
├── test/
│   ├── unit/
│   │   ├── models_test.dart
│   │   ├── providers_test.dart
│   │   └── validators_test.dart
│   ├── widget/
│   │   ├── screens_test.dart
│   │   └── widgets_test.dart
│   └── integration/
│       └── app_test.dart
│
└── android/
    └── app/
        └── src/main/AndroidManifest.xml
```

---

## State Management (Riverpod)

### Simple Provider (Read-only State)

```dart
// User info provider
final userProvider = Provider<User>((ref) {
  return User(
    id: '123',
    name: 'John Doe',
    email: 'john@company.com',
  );
});

// Usage
@override
Widget build(BuildContext context, WidgetRef ref) {
  final user = ref.watch(userProvider);
  
  return Text(user.name);
}
```

### State Notifier (Mutable State)

```dart
// Employee list state
final employeeListProvider = StateNotifierProvider.family<
    EmployeeListNotifier,
    AsyncValue<List<Employee>>,
    int  // company_id parameter
>((ref, companyId) {
  final repo = ref.watch(employeeRepositoryProvider);
  return EmployeeListNotifier(repo, companyId);
});

class EmployeeListNotifier extends StateNotifier<AsyncValue<List<Employee>>> {
  final EmployeeRepository _repository;
  final int _companyId;
  
  EmployeeListNotifier(this._repository, this._companyId)
      : super(const AsyncValue.loading());
  
  Future<void> loadEmployees() async {
    state = const AsyncValue.loading();
    
    try {
      final employees = await _repository.getEmployees(_companyId);
      state = AsyncValue.data(employees);
    } catch (e) {
      state = AsyncValue.error(e, StackTrace.current);
    }
  }
  
  Future<void> deleteEmployee(String employeeId) async {
    await _repository.deleteEmployee(employeeId);
    await loadEmployees();  // Refresh list
  }
}

// Usage
@override
Widget build(BuildContext context, WidgetRef ref) {
  final companyId = ref.watch(userProvider).companyId;
  final asyncEmployees = ref.watch(employeeListProvider(companyId));
  
  return asyncEmployees.when(
    data: (employees) => EmployeeListView(employees: employees),
    loading: () => const LoadingScreen(),
    error: (error, stack) => ErrorScreen(error: error),
  );
}
```

### Async Provider (Data Fetching)

```dart
// Fetch employee with network + cache
final employeeProvider = FutureProvider.family<Employee, String>((ref, employeeId) async {
  final repo = ref.watch(employeeRepositoryProvider);
  
  // Network call (with local caching)
  return await repo.getEmployee(employeeId);
});

// Usage with fallback to cache
final employeeWithCacheProvider = FutureProvider.family<Employee, String>((ref, employeeId) async {
  final local = ref.watch(localDatabaseProvider);
  
  try {
    // Try network first
    final employee = await ref.watch(employeeProvider(employeeId).future);
    
    // Update local cache
    await local.saveEmployee(employee);
    
    return employee;
  } catch (e) {
    // Fallback to local cache
    final cached = await local.getEmployee(employeeId);
    if (cached != null) return cached;
    rethrow;
  }
});
```

### Complex State with Logic

```dart
// Attendance notifier with check-in/check-out logic
final attendanceNotifierProvider = StateNotifierProvider<
    AttendanceNotifier,
    AsyncValue<AttendanceState>
>((ref) {
  final repo = ref.watch(attendanceRepositoryProvider);
  final gpsService = ref.watch(gpsServiceProvider);
  
  return AttendanceNotifier(repo, gpsService);
});

class AttendanceNotifier extends StateNotifier<AsyncValue<AttendanceState>> {
  final AttendanceRepository _repository;
  final GpsService _gpsService;
  
  AttendanceNotifier(this._repository, this._gpsService)
      : super(const AsyncValue.data(AttendanceState.initial()));
  
  Future<void> checkIn() async {
    state = const AsyncValue.loading();
    
    try {
      // Get GPS location
      final location = await _gpsService.getCurrentLocation();
      
      // Create check-in
      final checkin = Checkin(
        employeeId: '123',
        checkInTime: DateTime.now(),
        location: location,
      );
      
      // Save locally first (offline support)
      await _repository.saveCheckInLocal(checkin);
      
      // Sync to server
      await _repository.syncCheckIn(checkin);
      
      state = AsyncValue.data(AttendanceState.checkedIn(checkin));
    } catch (e) {
      state = AsyncValue.error(e, StackTrace.current);
    }
  }
  
  Future<void> checkOut() async {
    try {
      final location = await _gpsService.getCurrentLocation();
      
      final checkout = Checkout(
        employeeId: '123',
        checkOutTime: DateTime.now(),
        location: location,
      );
      
      await _repository.saveCheckOutLocal(checkout);
      await _repository.syncCheckOut(checkout);
      
      state = AsyncValue.data(AttendanceState.checkedOut(checkout));
    } catch (e) {
      state = AsyncValue.error(e, StackTrace.current);
    }
  }
}
```

---

## Navigation (GoRouter)

### Route Configuration

```dart
// lib/config/routes/app_routes.dart
final goRouterProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authProvider);
  
  return GoRouter(
    initialLocation: '/',
    redirect: (context, state) {
      final isLoggedIn = authState.isLoggedIn;
      final isLoggingIn = state.location == '/login';
      
      // Redirect to login if not authenticated
      if (!isLoggedIn && !isLoggingIn) {
        return '/login';
      }
      
      // Redirect to home if already logged in
      if (isLoggedIn && isLoggingIn) {
        return '/';
      }
      
      return null;
    },
    routes: <GoRoute>[
      GoRoute(
        path: '/',
        builder: (context, state) => const DashboardScreen(),
        routes: <GoRoute>[
          GoRoute(
            path: 'employees',
            builder: (context, state) => const EmployeeListScreen(),
            routes: <GoRoute>[
              GoRoute(
                path: ':employeeId',
                builder: (context, state) => EmployeeDetailScreen(
                  employeeId: state.pathParameters['employeeId']!,
                ),
              ),
              GoRoute(
                path: 'create',
                builder: (context, state) => const EmployeeFormScreen(),
              ),
            ],
          ),
          GoRoute(
            path: 'payroll',
            builder: (context, state) => const PayrollListScreen(),
            routes: <GoRoute>[
              GoRoute(
                path: ':payrollId',
                builder: (context, state) => PayslipScreen(
                  payrollId: state.pathParameters['payrollId']!,
                ),
              ),
            ],
          ),
          GoRoute(
            path: 'attendance',
            builder: (context, state) => const AttendanceScreen(),
          ),
          GoRoute(
            path: 'analytics',
            builder: (context, state) => const AnalyticsScreen(),
          ),
        ],
      ),
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/splash',
        builder: (context, state) => const SplashScreen(),
      ),
    ],
  );
});

// Usage in main.dart
@override
Widget build(BuildContext context, WidgetRef ref) {
  final goRouter = ref.watch(goRouterProvider);
  
  return MaterialApp.router(
    routerConfig: goRouter,
    theme: AppTheme.lightTheme,
  );
}
```

### Deep Linking

```dart
// Deep link: hranalytics://employee/EMP123
GoRoute(
  path: 'employee/:employeeId',
  builder: (context, state) => EmployeeDetailScreen(
    employeeId: state.pathParameters['employeeId']!,
  ),
),

// Deep link: hranalytics://payroll/2026-07
GoRoute(
  path: 'payroll/:payrollPeriod',
  builder: (context, state) => PayrollDetailScreen(
    payrollPeriod: state.pathParameters['payrollPeriod']!,
  ),
),
```

---

## API Integration

### HTTP Client Setup

```dart
// services/api/api_client.dart
final httpClientProvider = Provider<HttpClient>((ref) {
  return HttpClient(
    baseUrl: ref.watch(appConfigProvider).apiBaseUrl,
    interceptors: [
      AuthInterceptor(ref),
      LoggingInterceptor(),
      RetryInterceptor(),
    ],
  );
});

class AuthInterceptor extends Interceptor {
  final Ref _ref;
  
  AuthInterceptor(this._ref);
  
  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Add JWT token to every request
    final token = await _ref.read(secureStorageProvider).getToken();
    
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    
    handler.next(options);
  }
  
  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    // Handle 401 - refresh token
    if (err.response?.statusCode == 401) {
      final newToken = await _ref.read(authProvider.notifier).refreshToken();
      
      if (newToken != null) {
        // Retry original request with new token
        final opts = err.requestOptions;
        opts.headers['Authorization'] = 'Bearer $newToken';
        
        try {
          final response = await _ref.read(httpClientProvider).request<dynamic>(
            opts.path,
            options: Options(
              method: opts.method,
              headers: opts.headers,
            ),
            data: opts.data,
          );
          handler.resolve(response);
        } catch (e) {
          handler.next(err);
        }
      }
    }
    
    handler.next(err);
  }
}
```

### Repository Pattern

```dart
// repositories/employee_repository.dart
abstract class EmployeeRepository {
  Future<List<Employee>> getEmployees(int companyId);
  Future<Employee> getEmployee(String employeeId);
  Future<Employee> createEmployee(CreateEmployeeRequest request);
  Future<Employee> updateEmployee(String employeeId, UpdateEmployeeRequest request);
  Future<void> deleteEmployee(String employeeId);
}

class EmployeeRepositoryImpl implements EmployeeRepository {
  final HttpClient _httpClient;
  final LocalDatabase _localDb;
  final Dio _dio;
  
  EmployeeRepositoryImpl(this._httpClient, this._localDb, this._dio);
  
  @override
  Future<List<Employee>> getEmployees(int companyId) async {
    try {
      // Try network first
      final response = await _httpClient.get(
        '/api/v1/employees',
        queryParameters: {'companyId': companyId},
      );
      
      final employees = (response.data as List)
          .map((e) => Employee.fromJson(e as Map<String, dynamic>))
          .toList();
      
      // Cache locally
      await _localDb.saveEmployees(employees);
      
      return employees;
    } on DioException catch (e) {
      // Fallback to local cache
      if (e.type == DioExceptionType.connectionTimeout) {
        final cached = await _localDb.getEmployees(companyId);
        if (cached.isNotEmpty) return cached;
      }
      
      rethrow;
    }
  }
  
  @override
  Future<Employee> getEmployee(String employeeId) async {
    try {
      final response = await _httpClient.get('/api/v1/employees/$employeeId');
      final employee = Employee.fromJson(response.data);
      
      await _localDb.saveEmployee(employee);
      return employee;
    } catch (e) {
      // Try local cache
      final cached = await _localDb.getEmployee(employeeId);
      if (cached != null) return cached;
      rethrow;
    }
  }
  
  @override
  Future<Employee> createEmployee(CreateEmployeeRequest request) async {
    final response = await _httpClient.post(
      '/api/v1/employees',
      data: request.toJson(),
    );
    
    return Employee.fromJson(response.data);
  }
}
```

---

## Offline Sync Architecture

### Sync Service

```dart
// services/sync/sync_service.dart
final syncServiceProvider = Provider<SyncService>((ref) {
  return SyncService(
    localDatabase: ref.watch(localDatabaseProvider),
    repository: ref.watch(employeeRepositoryProvider),
    connectivity: ref.watch(connectivityProvider),
  );
});

class SyncService {
  final LocalDatabase _localDb;
  final EmployeeRepository _repository;
  final ConnectivityService _connectivity;
  
  final _syncQueue = <SyncItem>[];
  
  Future<void> syncPendingChanges() async {
    if (!await _connectivity.isConnected) return;
    
    // Get all pending changes from local database
    final pendingChanges = await _localDb.getPendingChanges();
    
    for (final change in pendingChanges) {
      try {
        switch (change.operation) {
          case SyncOperation.create:
            await _repository.createEmployee(change.data);
            break;
          case SyncOperation.update:
            await _repository.updateEmployee(change.id, change.data);
            break;
          case SyncOperation.delete:
            await _repository.deleteEmployee(change.id);
            break;
        }
        
        // Mark as synced
        await _localDb.markAsSynced(change.id);
      } catch (e) {
        // Retry later
        _syncQueue.add(change);
        await _localDb.incrementRetryCount(change.id);
      }
    }
  }
}
```

---

**Last Updated:** July 2026
**Status:** Mobile Architecture Complete
