import json
import urllib.request
import urllib.error
import http.cookiejar
import sqlite3

BASE_URL = "http://localhost:5088"
DB_PATH = r"C:\Users\f0rz1\Desktop\lab\net\ParkingManagement\Server\ServerApp\parking.db"

results = []

def record(name, passed, details=""):
    status = "PASS" if passed else "FAIL"
    results.append({"test": name, "status": status, "details": details})
    print(f"[{status}] {name}: {details}")

# 1. Тест веб-аутентифікації адміністратора
def test_admin_login():
    cj = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))
    data = urllib.parse.urlencode({
        "Email": "admin@parking.com",
        "Password": "AdminPassword123!"
    }).encode("utf-8")
    req = urllib.request.Request(f"{BASE_URL}/auth/login", data=data)
    try:
        resp = opener.open(req)
        cookies = [c.name for c in cj]
        has_token = "jwt_token" in cookies
        record("Вхід адміністратора через веб-форму (/auth/login)", has_token, f"Кукі отримано: {cookies}")
    except Exception as e:
        record("Вхід адміністратора через веб-форму (/auth/login)", False, str(e))

# 2. Тест захисту API (запит без токена)
def test_unauthorized_api():
    req = urllib.request.Request(f"{BASE_URL}/api/items")
    try:
        urllib.request.urlopen(req)
        record("Захист API без токена ([Authorize])", False, "Очікувався статус 401, але запит пройшов")
    except urllib.error.HTTPError as e:
        record("Захист API без токена ([Authorize])", e.code == 401, f"Статус код: {e.code} Unauthorized")
    except Exception as e:
        record("Захист API без токена ([Authorize])", False, str(e))

# 3. Тест API логіну користувача
def test_user_api_login():
    data = json.dumps({"email": "sasha@parking.com", "password": "Password123!"}).encode("utf-8")
    req = urllib.request.Request(f"{BASE_URL}/api/auth/login", data=data, headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req) as resp:
            body = json.loads(resp.read().decode("utf-8"))
            token = body.get("token")
            is_valid = token is not None and body.get("role") == "User" and body.get("name") == "Sasha Yaschenko"
            record("API логін працівника (POST /api/auth/login)", is_valid, f"Отримано токен для {body.get('name')}")
            return token
    except Exception as e:
        record("API логін працівника (POST /api/auth/login)", False, str(e))
        return None

# 4. Тест відхилення невірного паролю через BCrypt
def test_invalid_password_login():
    data = json.dumps({"email": "sasha@parking.com", "password": "WrongPassword123"}).encode("utf-8")
    req = urllib.request.Request(f"{BASE_URL}/api/auth/login", data=data, headers={"Content-Type": "application/json"})
    try:
        urllib.request.urlopen(req)
        record("Перевірка відхилення невірного пароля через BCrypt", False, "Запит пройшов замість 401")
    except urllib.error.HTTPError as e:
        record("Перевірка відхилення невірного пароля через BCrypt", e.code == 401, f"Код: {e.code} Unauthorized")
    except Exception as e:
        record("Перевірка відхилення невірного пароля через BCrypt", False, str(e))

# 5. Тест отримання списку місць через API
def test_get_spots(token):
    req = urllib.request.Request(f"{BASE_URL}/api/items", headers={"Authorization": f"Bearer {token}"})
    try:
        with urllib.request.urlopen(req) as resp:
            spots = json.loads(resp.read().decode("utf-8"))
            record("Отримання списку паркомісць (GET /api/items)", len(spots) > 0, f"Отримано {len(spots)} місць")
            return spots
    except Exception as e:
        record("Отримання списку паркомісць (GET /api/items)", False, str(e))
        return []

# 6. Тест бронювання місця
def test_create_reservation(token, spot_id):
    data = json.dumps({"spotID": spot_id}).encode("utf-8")
    req = urllib.request.Request(f"{BASE_URL}/api/actions", data=data, headers={
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    })
    try:
        with urllib.request.urlopen(req) as resp:
            body = json.loads(resp.read().decode("utf-8"))
            record("Створення бронювання місця (POST /api/actions)", resp.status == 201, f"Бронь № {body.get('id')} створена")
            return True
    except Exception as e:
        record("Створення бронювання місця (POST /api/actions)", False, str(e))
        return False

# 7. Тест блокування повторного бронювання
def test_duplicate_reservation(token, spot_id):
    data = json.dumps({"spotID": spot_id}).encode("utf-8")
    req = urllib.request.Request(f"{BASE_URL}/api/actions", data=data, headers={
        "Content-Type": "application/json",
        "Authorization": f"Bearer {token}"
    })
    try:
        urllib.request.urlopen(req)
        record("Блокування повторного бронювання зайнятого місця", False, "Дозволило забронювати зайняте")
    except urllib.error.HTTPError as e:
        record("Блокування повторного бронювання зайнятого місця", e.code == 400, f"Код помилки: {e.code} Bad Request")
    except Exception as e:
        record("Блокування повторного бронювання зайнятого місця", False, str(e))

# 8. Тест скасування бронювання та звільнення місця
def test_cancel_reservation(token, spot_id):
    req = urllib.request.Request(f"{BASE_URL}/api/actions/{spot_id}", method="DELETE", headers={
        "Authorization": f"Bearer {token}"
    })
    try:
        with urllib.request.urlopen(req) as resp:
            record("Скасування бронювання та звільнення місця (DELETE /api/actions/{id})", resp.status == 200, "Місце успішно звільнено")
    except Exception as e:
        record("Скасування бронювання та звільнення місця (DELETE /api/actions/{id})", False, str(e))

# 9. Тест цілісності зовнішніх ключів у SQLite
def test_database_fk_integrity():
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        cursor.execute("PRAGMA foreign_key_check;")
        fk_errors = cursor.fetchall()
        record("Перевірка зовнішніх ключів БД (PRAGMA foreign_key_check)", len(fk_errors) == 0, f"Помилок FK: {len(fk_errors)}")
        conn.close()
    except Exception as e:
        record("Перевірка зовнішніх ключів БД (PRAGMA foreign_key_check)", False, str(e))

if __name__ == "__main__":
    print("=== ЗАПУСК КОМПЛЕКСНОГО ТЕСТУВАННЯ СИСТЕМИ (ЛАБОРАТОРНА 10) ===")
    test_admin_login()
    test_unauthorized_api()
    token = test_user_api_login()
    test_invalid_password_login()
    spots = test_get_spots(token) if token else []
    
    # Шукаємо перше вільне місце для тесту бронювання
    free_spot = next((s for s in spots if s.get("status") == "Free"), None)
    if free_spot and token:
        spot_id = free_spot["id"]
        if test_create_reservation(token, spot_id):
            test_duplicate_reservation(token, spot_id)
            test_cancel_reservation(token, spot_id)
    
    test_database_fk_integrity()
    print("=== ТЕСТУВАННЯ ЗАВЕРШЕНО ===")
