# Mobile Security & Compliance

Comprehensive guide to authentication, data protection, offline security, and regulatory compliance.

---

## Authentication Flow

### Biometric + PIN Authentication

```dart
// Biometric Authentication
final biometricAuthProvider = FutureProvider<bool>((ref) async {
  final localAuth = LocalAuthentication();
  
  try {
    final canAuthenticate = await localAuth.canCheckBiometrics;
    if (!canAuthenticate) return false;
    
    return await localAuth.authenticate(
      localizedReason: 'Authenticate to access HR Analytics',
      options: const AuthenticationOptions(
        stickyAuth: true,
        biometricOnly: true,
      ),
    );
  } catch (e) {
    return false;
  }
});

// Login Screen Implementation
class LoginScreen extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authProvider);
    
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Email/Password fields
            TextField(
              decoration: InputDecoration(hintText: 'Email'),
              onChanged: (email) => ref
                  .read(authProvider.notifier)
                  .setEmail(email),
            ),
            TextField(
              obscureText: true,
              decoration: InputDecoration(hintText: 'Password'),
              onChanged: (password) => ref
                  .read(authProvider.notifier)
                  .setPassword(password),
            ),
            
            // Login button
            ElevatedButton(
              onPressed: () async {
                try {
                  await ref
                      .read(authProvider.notifier)
                      .login();
                } catch (e) {
                  // Show error
                }
              },
              child: const Text('Login'),
            ),
            
            // Biometric login
            IconButton(
              icon: const Icon(Icons.fingerprint),
              onPressed: () async {
                final success = await ref
                    .read(biometricAuthProvider.future);
                
                if (success) {
                  await ref
                      .read(authProvider.notifier)
                      .biometricLogin();
                }
              },
            ),
          ],
        ),
      ),
    );
  }
}

// Auth Service
class AuthService {
  final SecureStorageService _secureStorage;
  final Dio _dio;
  
  Future<AuthResponse> login(String email, String password) async {
    // 1. Send credentials to backend (HTTPS only)
    final response = await _dio.post('/auth/login', data: {
      'email': email,
      'password': password,
    });
    
    final authResponse = AuthResponse.fromJson(response.data);
    
    // 2. Store tokens securely
    await _secureStorage.saveToken(authResponse.accessToken);
    await _secureStorage.saveRefreshToken(authResponse.refreshToken);
    
    // 3. Return success
    return authResponse;
  }
  
  Future<void> biometricLogin() async {
    // 1. Authenticate locally with biometric
    final isAuthenticated = await LocalAuthentication().authenticate(
      localizedReason: 'Authenticate to access app',
      options: const AuthenticationOptions(biometricOnly: true),
    );
    
    if (!isAuthenticated) throw UnauthorizedException(message: 'Biometric failed');
    
    // 2. Retrieve stored credentials securely
    final email = await _secureStorage.getEmail();
    final password = await _secureStorage.getPassword();
    
    // 3. Verify with backend (token refresh)
    final refreshToken = await _secureStorage.getRefreshToken();
    
    final response = await _dio.post('/auth/refresh', data: {
      'refreshToken': refreshToken,
    });
    
    await _secureStorage.saveToken(response.data['accessToken']);
  }
}
```

---

## Secure Storage

### Encrypt Sensitive Data

```dart
// services/secure_storage_service.dart
class SecureStorageService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage(
    aOptions: AndroidOptions(
      storageCiphertext: true,  // Encrypt data
      resetOnError: true,
    ),
    iOptions: IOSOptions(
      accessibility: KeychainAccessibility.first_this_device_this_code,
    ),
  );
  
  Future<void> saveToken(String token) async {
    await _storage.write(
      key: 'access_token',
      value: token,
    );
  }
  
  Future<String?> getToken() async {
    return await _storage.read(key: 'access_token');
  }
  
  Future<void> saveSensitiveData(String key, String value) async {
    // Encrypt with AES-256
    final encrypted = _encryptAES256(value);
    
    await _storage.write(
      key: key,
      value: encrypted,
    );
  }
  
  Future<String?> getSensitiveData(String key) async {
    final encrypted = await _storage.read(key: key);
    if (encrypted == null) return null;
    
    return _decryptAES256(encrypted);
  }
  
  String _encryptAES256(String plaintext) {
    // Implementation using encrypt package
    final key = encrypt.Key.fromLength(32);
    final encrypter = encrypt.Encrypter(encrypt.AES(key));
    final encrypted = encrypter.encrypt(plaintext, iv: encrypt.IV.fromLength(16));
    return encrypted.base64;
  }
  
  String _decryptAES256(String ciphertext) {
    final key = encrypt.Key.fromLength(32);
    final encrypter = encrypt.Encrypter(encrypt.AES(key));
    final decrypted = encrypter.decrypt64(ciphertext);
    return decrypted;
  }
}
```

---

## SSL Pinning

### Certificate Pinning

```dart
// Prevent man-in-the-middle attacks
final httpClientProvider = Provider<Dio>((ref) {
  final dio = Dio();
  
  // Add certificate pinning
  (dio.httpClientAdapter as DefaultHttpClientAdapter).onHttpClientCreate = (HttpClient client) {
    client.badCertificateCallback = (cert, host, port) {
      // Only accept our certificate
      if (host == 'api.hranalytics.com') {
        return _verifyCertificate(cert, host);
      }
      return false;
    };
  };
  
  return dio;
});

bool _verifyCertificate(X509Certificate cert, String host) {
  // Verify certificate hash
  final publicKeyHash = _getSHA256Hash(cert.der);
  const expectedHash = 'abc123def456...';  // Your certificate hash
  
  return publicKeyHash == expectedHash;
}
```

---

## Data Encryption at Rest

### Encrypt Local Database

```dart
// Create encrypted SQLite database
final localDatabaseProvider = Provider<Database>((ref) async {
  final databasesPath = await getDatabasesPath();
  final path = join(databasesPath, 'hr_analytics.db');
  
  // Open with encryption (SQLCipher)
  final database = await openDatabase(
    path,
    password: _getDatabasePassword(),  // From secure storage
    version: 1,
    onCreate: (Database db, int version) async {
      await db.execute('CREATE TABLE employees (...)');
    },
  );
  
  return database;
});

String _getDatabasePassword() {
  // Password should be stored securely
  // Or generated from device unique identifier + user ID
  return 'complex_random_password_${deviceId}_${userId}';
}

// Encrypt sensitive fields before storing
Future<void> saveEmployeeData(Employee employee) async {
  final db = await getDatabase();
  
  final encryptedEmployee = Employee(
    id: employee.id,
    firstName: _encrypt(employee.firstName),
    lastName: _encrypt(employee.lastName),
    email: _encrypt(employee.email),
    baseSalary: _encrypt(employee.baseSalary.toString()),
  );
  
  await db.insert('employees', encryptedEmployee.toMap());
}

String _encrypt(String plaintext) {
  // Use Fernet symmetric encryption
  final key = _getEncryptionKey();
  final fernet = Fernet(key);
  return fernet.encrypt(plaintext);
}

String _decrypt(String ciphertext) {
  final key = _getEncryptionKey();
  final fernet = Fernet(key);
  return fernet.decrypt(ciphertext);
}
```

---

## GDPR Compliance

### Data Export

```dart
// Export user data (DSAR - Data Subject Access Request)
final dataExportProvider = FutureProvider<DataExport>((ref) async {
  final userId = ref.watch(currentUserProvider).id;
  
  // Collect all user data
  final employees = await ref.read(employeeRepositoryProvider).getMyData();
  final payroll = await ref.read(payrollRepositoryProvider).getMyPayroll();
  const attendance = await ref.read(attendanceRepositoryProvider).getMyAttendance();
  
  // Create export
  const export = DataExport(
    employees: employees,
    payroll: payroll,
    attendance: attendance,
    exportDate: DateTime.now(),
  );
  
  // Save to file (encrypted)
  await _saveExportToFile(export);
  
  return export;
});

Future<void> _saveExportToFile(DataExport export) async {
  final json = jsonEncode(export.toJson());
  
  // Encrypt file
  final encrypted = _encryptAES256(json);
  
  // Save to app directory
  final appDir = await getApplicationDocumentsDirectory();
  final file = File('${appDir.path}/data_export.json.encrypted');
  
  await file.writeAsString(encrypted);
}
```

### Data Deletion (Right to Be Forgotten)

```dart
// Delete all user data
final deleteUserDataProvider = FutureProvider<void>((ref) async {
  final userId = ref.watch(currentUserProvider).id;
  
  // Step 1: Request deletion from backend
  await ref.read(authRepositoryProvider).requestDataDeletion();
  
  // Step 2: Wait for backend confirmation (24-48 hours)
  // Step 3: Clear local data
  
  // Clear local database
  final db = await ref.read(localDatabaseProvider);
  await db.delete('employees');
  await db.delete('payroll');
  await db.delete('attendance');
  
  // Clear secure storage
  await ref.read(secureStorageProvider).deleteAll();
  
  // Clear app cache
  await ref.read(cacheProvider).clear();
  
  // Clear browser cookies (if webview used)
  await CookieManager.clearCookies();
  
  // Log out
  await ref.read(authProvider.notifier).logout();
});
```

### Consent Management

```dart
// Track user consent for data processing
class ConsentService {
  Future<void> requestConsent() async {
    final showDialog = await _shouldShowConsentDialog();
    
    if (!showDialog) return;
    
    const consentDialog = AlertDialog(
      title: Text('Privacy Policy'),
      content: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('We process your data for:'),
            Text('1. HR Analytics - performance analysis'),
            Text('2. Payroll - salary calculation'),
            Text('3. Compliance - regulatory requirements'),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => _rejectConsent(),
          child: const Text('Reject'),
        ),
        TextButton(
          onPressed: () => _acceptConsent(),
          child: const Text('Accept'),
        ),
      ],
    );
    
    showDialog(context: context, builder: (_) => consentDialog);
  }
  
  Future<void> _acceptConsent() async {
    await _secureStorage.write(
      key: 'consent_accepted',
      value: jsonEncode({
        'accepted': true,
        'date': DateTime.now().toIso8601String(),
        'version': '1.0',
      }),
    );
  }
}
```

---

## Network Security

### HTTPS Enforcement

```dart
// Force HTTPS connections
final httpClientProvider = Provider<Dio>((ref) {
  final dio = Dio(BaseOptions(
    baseUrl: 'https://api.hranalytics.com',  // Only HTTPS
  ));
  
  dio.interceptors.add(
    HttpsOnlyInterceptor(),
  );
  
  return dio;
});

class HttpsOnlyInterceptor extends Interceptor {
  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    if (!options.uri.scheme.contains('https')) {
      throw SecurityException(message: 'Only HTTPS connections allowed');
    }
    handler.next(options);
  }
}

// Disable HTTP, only allow HTTPS
final httpClient = HttpClient()
  ..badCertificateCallback = (cert, host, port) => false;
```

### API Request Signing

```dart
// Sign all API requests with HMAC-SHA256
class RequestSigningInterceptor extends Interceptor {
  final String _apiSecret;
  
  RequestSigningInterceptor(this._apiSecret);
  
  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Create signature
    final timestamp = DateTime.now().millisecondsSinceEpoch.toString();
    final signature = _createSignature(
      method: options.method,
      path: options.path,
      timestamp: timestamp,
      body: options.data,
    );
    
    // Add to headers
    options.headers['X-Signature'] = signature;
    options.headers['X-Timestamp'] = timestamp;
    
    handler.next(options);
  }
  
  String _createSignature({
    required String method,
    required String path,
    required String timestamp,
    required dynamic body,
  }) {
    final bodyStr = body is String ? body : jsonEncode(body);
    final message = '$method|$path|$timestamp|$bodyStr';
    
    final key = utf8.encode(_apiSecret);
    final bytes = utf8.encode(message);
    
    final hmac = Hmac(sha256, key);
    final digest = hmac.convert(bytes);
    
    return digest.toString();
  }
}
```

---

## Offline Security

### Secure Offline Sync

```dart
// Queue changes while offline, sync securely when online
class OfflineSyncService {
  final LocalDatabaseService _localDb;
  final Dio _dio;
  final ConnectivityService _connectivity;
  
  Future<void> queueChange(SyncItem item) async {
    // Encrypt sensitive data
    const encrypted = _encryptSensitiveFields(item);
    
    // Store in local queue (encrypted)
    await _localDb.addToSyncQueue(encrypted);
  }
  
  Future<void> syncWhenOnline() async {
    // Listen for connectivity changes
    _connectivity.onConnectivityChanged.listen((isConnected) async {
      if (isConnected) {
        await _syncQueuedChanges();
      }
    });
  }
  
  Future<void> _syncQueuedChanges() async {
    final queuedItems = await _localDb.getSyncQueue();
    
    for (final item in queuedItems) {
      try {
        // Verify request signing
        final signed = _signRequest(item);
        
        // Send to backend
        await _dio.post(
          '/api/v1/sync',
          data: signed,
          options: Options(
            headers: {'X-Offline-Sync': 'true'},
          ),
        );
        
        // Remove from queue
        await _localDb.removeSyncItem(item.id);
      } catch (e) {
        // Keep in queue for retry
        await _localDb.incrementRetryCount(item.id);
      }
    }
  }
}
```

---

## Security Checklist

```
Authentication
□ Biometric + PIN implemented
□ Token refresh mechanism working
□ Session timeout enforced
□ Logout clears all data

Data Protection
□ All sensitive data encrypted
□ SSL pinning implemented
□ HTTPS only (no HTTP)
□ Database encrypted (SQLCipher)

Storage
□ Tokens in secure storage (not SharedPreferences)
□ No credentials in logs
□ No sensitive data in cache
□ App-specific directories used

Network
□ Certificate pinning active
□ HTTPS enforced
□ API requests signed
□ Retry logic secure

Compliance
□ GDPR deletion implemented
□ Data export working
□ Consent management in place
□ Audit logging enabled

Testing
□ Security tests automated
□ Penetration testing done
□ Dependency vulnerabilities checked
□ Code signing certificates valid
```

---

**Last Updated:** July 2026
**Status:** Mobile Security Complete
