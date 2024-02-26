ASP.NET Core Application created for making online bookings. Project was designed using Clean Architecture.
<br />
I had an invaluable opportunity to work on this project and learn from elite professional Bhrugen Patel.

**User features**<br />
- Making bookings based on check in date and number of nights
- Browsing through available villas
- Managing own booking
- Online payments 
- Invoice in pdf and word format download
- Villa details in pptx format download

**Admin features**<br />
- Managing all bookings
- Villa CRUD operations
- Villa number CRUD operations
- Villa amenity CRUD operations
- Dashboard with various charts about user activity, booking actiity, total revenue

**System Requirements**<br />
- .NET 8
- Microsoft Visual Studio 2022
- SQL Server 2019, 2022<br /><br />

**Bookify.Application**<br />
Frameworks
- Microsoft.AspNetCore.App
- Microsoft.NETCore.App

Packages
- Microsoft.AspNetCore.Hosting<br /><br />


**Bookify.Domain**<br />
Frameworks
- Microsoft.NETCore.App

Packages
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.AspNetCore.Mvc.Core<br /><br />


**Bookify.Infrastructure**<br />
Frameworks
- Microsoft.NETCore.App

Packages
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools<br /><br />

**Bookify.Web**<br />
Frameworks
- Microsoft.AspNetCore.App
- Microsoft.NETCore.App

Packages
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Stripe.net
- Syncfusion.DocIO.Net.Core
- Syncfusion.DocIORenderer.Net.Core
- Syncfusion.Licensing
- Syncfusion.Pdf.Net.Core
- Syncfusion.Presentation.Net.Core

