# IT Destek Paneli - Ticket Sistemi Web API

## Proje Amacı

Kullanıcıların yaşadıkları sorunlarla ilgili ticket açabildiği, yetkililerin ise bu ticketlara cevap verip işlemi tamamlayabildiği bir destek paneli Web API projesidir.

Kullanıcı sisteme kayıt olup giriş yaptıktan sonra yaşadığı sorunla ilgili bir ticket oluşturur. Yetkili kişi oluşturulan ticketları görüntüleyebilir, ticketlara cevap yazabilir ve sorun çözüldüğünde ticketı kapatabilir.

Ticketı oluşturan kullanıcı da kendi ticketındaki cevapları görüntüleyebilir ve karşılık verebilir. Ticket kapatıldıktan sonra kullanıcı aldığı desteği 1 ile 5 arasında puanlayarak değerlendirebilir.

Ayrıca ticket içerisinde bulunan mesajların tamamı görüntülenebilir. Mesaj geçmişi isteğe göre en eski mesajdan en yeni mesaja veya en yeni mesajdan en eski mesaja doğru sıralanabilir.

## Özellikler

- Kullanıcı kaydı ve girişi (şifreler BCrypt ile hashlenir)
- Kullanıcı / Yetkili rol ayrımı
- Ticket açma, listeleme ve detay görüntüleme
- Kullanıcı sadece kendi ticketlarını, yetkili tüm ticketları görür
- Ticketa cevap yazma ve mesaj geçmişini sıralı görüntüleme
- Ticket kapatma (sadece yetkili)
- Kapatılan ticketı değerlendirme (sadece ticket sahibi, 1-5 puan)
- Ticket silme (soft delete)

## Kullanılan Kütüphaneler

- Microsoft.EntityFrameworkCore.SqlServer: SQL Server bağlantısı
- Microsoft.EntityFrameworkCore.Tools: Add-Migration ve Update-Database komutları
- Carter: Endpointleri UserModule, TicketModule gibi ayrı dosyalara bölmek
- Mapster: DTOyu entityye çevirmek
- Ardalis.SmartEnum: Rol (Kullanıcı/Yetkili) ve durum (Açık/Cevaplandı/Kapatıldı) alanları
- TS.Result: Tüm cevapların aynı formatta dönmesi (data, errorMessages, isSuccessful, statusCode)
- BCrypt.Net-Next: Şifreleri hashleyerek saklamak ve girişte doğrulamak
- Scalar.AspNetCore: APIyi tarayıcıdan test etmek için kullanılan arayüz

## Geliştirme Süreci

Projeyi geliştirmeye başlarken öncelikle hocamızın derste yaptığı Hasta Kayıt projesine baktım. Özellikle klasörlerin nasıl oluşturulduğunu, hangi paketlerin kullanıldığını ve endpointlerin nasıl yazıldığını inceleyerek kendi projemde de benzer bir yapı kurmaya çalıştım.

Kodlamaya başlamadan önce veritabanında hangi tablolara ihtiyacım olacağını düşündüm. Bu proje için kullanıcılar, ticketlar ve ticketlara yazılan cevaplar olmak üzere üç tablonun yeterli olacağına karar verdim. Cevapları doğrudan ticket tablosunda tutmak yerine ayrı bir tabloda tuttum. Çünkü bir ticket için birden fazla cevap yazılabiliyor ve bu cevapların hangi sırayla yazıldığının korunması gerekiyor.

Daha sonra bütün tablolarda ortak olarak kullanacağım alanları düşündüm. Id, oluşturulma tarihi, güncellenme tarihi ve silinme tarihi gibi alanları tekrar tekrar yazmamak için Entity adında bir temel sınıf oluşturdum ve diğer modelleri bundan türettim. Kullanıcı rolleri ve ticket durumları için de derste hocamızın kullandığı SmartEnum yapısını kullandım.

DbContext kısmında tablolar arasındaki ilişkileri oluşturdum. Ayrıca silinen kayıtların normal sorgularda gelmemesi için gerekli filtreyi ekledim ve tarih alanlarının otomatik olarak doldurulmasını ayarladım. Daha sonra migration oluşturarak veritabanını kod üzerinden oluşturdum.

Kullanıcı kayıtlarında şifreleri direkt olarak veritabanına kaydetmek yerine BCrypt kullanarak hashledim. Sisteme kayıt olan kullanıcıların tamamı normal kullanıcı olarak oluşturuluyor. İlk yetkili kullanıcıyı ise uygulama çalışırken appsettings.json içerisinde bulunan bilgilerden otomatik olarak oluşturdum. Böylece kayıt ekranından herkesin kendisini yetkili olarak oluşturmasının önüne geçtim.

Ticket işlemlerini endpoint endpoint yazdım. Yazdığım her işlemden sonra Scalar üzerinden test ederek çalışıp çalışmadığını kontrol ettim. Testlerde sadece işlemin başarılı olup olmadığına bakmadım. Yetkisiz bir işlemde 403, kayıt bulunamadığında 404 ve gönderilen istek kurallara uymadığında 400 dönüp dönmediğini de kontrol ettim.

Son olarak derste işlediğimiz konuları tekrar kontrol ettim. Projede eksik kalan Exception Handler ve DELETE işlemlerini de ekledim. DELETE işleminde kayıtları tamamen silmek yerine soft delete mantığını kullandım.

Projeyi geliştirirken anlamadığım veya hata aldığım bazı konularda yapay zekâdan (Claude) açıklama ve yönlendirme desteği aldım. Özellikle takıldığım noktaları anlamak ve hatalarımın nedenini bulmak için kullandım. Kodun yazılması, düzenlenmesi ve test edilmesi tarafını kendim yaptım.