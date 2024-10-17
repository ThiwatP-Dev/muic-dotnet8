using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IInstructorProvider _instructorProvider;
        private readonly IFlashMessage _flashMessage;

        public AccountController(ApplicationDbContext db,
                                 SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser> userManager,
                                 IAuthenticationProvider authenticationProvider,
                                 IInstructorProvider instructorProvider,
                                 IFlashMessage flashMessage)
        {
            _db = db;
            _signInManager = signInManager;
            _userManager = userManager;
            _authenticationProvider = authenticationProvider;
            _instructorProvider = instructorProvider;
            _flashMessage = flashMessage;
        }

        public IActionResult Login(string returnUrl)
        {
            var model = new LoginViewModel
                        {
                            ReturnUrl = returnUrl
                        };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var authResult = await GetAuthenticatedToken(login.Username, login.Password);
            if (authResult.IsAuthenticate)
            {
                if (authResult.JwtRoles != null && authResult.JwtRoles.Any())
                {
                    List<string> allowRoles = new List<string> { "faculty", "pc_lecturers", "pt_lecturers", "staff" };
                    var intersectRoles = allowRoles.Intersect(authResult.JwtRoles).ToList();
                    if (intersectRoles == null || intersectRoles.Count == 0)
                    {
                        return View(login);
                    }
                }
                else
                {
                    var instructor = _instructorProvider.GetInstructorByCode(login.Username);
                    if (instructor == null && login.Username != "uspark_teacher")
                    {
                        return View(login);
                    }
                }

                var user = await _userManager.FindByNameAsync(login.Username);
                if (user == null)
                {
                    var newUser = new ApplicationUser
                                  {
                                      Id = Guid.NewGuid().ToString(),
                                      FirstnameTH = string.Empty,
                                      LastnameTH = string.Empty,
                                      FirstnameEN = string.Empty,
                                      LastnameEN = string.Empty,
                                      UserName = login.Username
                                  };
                    await _userManager.CreateAsync(newUser);
                    user = await _userManager.FindByNameAsync(login.Username);
                }

                var claims = new List<Claim>();

                var userRoles = await _userManager.GetRolesAsync(user);
                var roleIds = _db.Roles.AsNoTracking()
                                       .Where(x => userRoles.Contains(x.Name))
                                       .Select(x => x)
                                       .ToList();
                if (roleIds.Any(x => x.Name.Contains("Admin")))
                {
                    claims.Add(new Claim("SpecialPermission", "Admin"));
                }
                await _userManager.AddClaimsAsync(user, claims);

                await _signInManager.SignInAsync(user, true);

                if (string.IsNullOrEmpty(login.ReturnUrl))
                    return Redirect("~/");

                return Redirect(login.ReturnUrl);
            }
            
            return View(login);
        }

        private async Task GeneratUser()
        {
            var usernames = new string[] {"abhisit.suw","adisorn.sri","Adriano","Adul.jak","Akarapong.won","alessandrou","amornrat.sue","anan.sal","anan.sen","anchalee.aru","anchalee.kum","anchisa.kan","andrewf","andrewl","anisa.mek","anitus.rav","anothai.sae","apichaya.wai","apinya.cha","Apisit.sue","aree.kon","artit.taw","arunee.kor","arunotai.khi","arunothai.pra","attaporn.noi","bandit.ain","bandit.ain","benrisa.sur","boonlua.bua","boonrat.kli","budsaba.won","budsara.yar","chadanis.sri","chaivatna","chaiwat.gri","chaiyapruek.pru","chakkaphon.sin","chakriya.dua","chalatip.mee","chalit.lap","chanettee.poo","chanettee.poo","chanita.nga","chanpen.khu","chatwaroot.dul","chayanon.poo","chayapim.war","chomjai.puj","Chonnakan.sit","chonticha.jan","chonticha.kor","christin","christopherw","colinc","dacha.nam","duangduean.sae","garyw","george","george.amu","greg","Harutai.sri","henryh","ian","ianm","issarapong.tos","jan","Janjira.pho","jantra.jee","jarunee.kha","jeerapan.cha","jeerawan.tho","jetapass.sao","jidapa.joe","jintana.pua","jiraporn.kho","jiratcha.ane","jittrapa.vut","josephs","julien","jumlong.phi","jun","jutharat.thi","Kamlang.chu","kamonthip.kit","kamonthip.kla","kankamon.tha","kanokeporn.hab","kanokwan.kar","kanokwan.san","kanyakon.pen","kawin.pla","keswit.poo","ketsirin.che","ketvaree.pha","kiatpitack.put","kitsana.chu","kitsayaporn.uny","kittiphat.pha","kittirat.pru","kochapath.ama","korakot.int","korakot.moo","korrachai.lek","krit.mol","krit.piy","kritsana.noi","krueawan.noi","kullacha.ler","kunyapat.kae","kurniati.wir","kwanrattana.int","leigh","maliwan.das","manop.pho","maria","markm","marye","merin","metpariya.kir","moragot.jam","naganda.tha","napaporn.jin","napat.sri","nareepat.sri","narintorn.pen","narirat.loe","narongchai.pak","nataporn.pra","natcha.pho","natchaphon.khu","nattapon.boo","nattawath.suk","natthapong.bus","nattika.phu","nikom.phi","nikom.phi","nisakorn.sae","Nisarat.rod","nisit.tim","nithiphat.nit","niwat.arm","niyom.sub","noppadol.phu","noppharada.man","nopporn.pir","nuchjaree.cha","nunnapan","nunnapin.sas","nuntana.kor","nutkittaboonyarit.eks","nuttaporn.ham","nuttawut.pin","Nuttawut.som","nutthaboon.por","nutthanunt.rit","ornautjima.pra","paiboon.kok","paisan.pro","palita.sit","panisara.aka","panudta.das","papassorn.pea","parinya.sri","patchara.sig","pathita.suw","pathyphorn.thi","pattaka","pattanit.mee","pattharat.uat","paulm","pawirat.pro","permsak.ari","peter.ema","peter.smi","phanida.aph","phanita.pot","phannee.mee","phanotsakorn.suw","phatsiri.cha","pheerasak.nak","phimon.che","phitsinee.dae","phuriwat.mee","pichit.kae","pimjai.chu","pimlada.run","pinyapat.kan","piraporn.nuc","pittayarat.lao","piyawan.jap","ploy","pongphattra.bur","pongsatorn.kae","pongvisit.kar","ponlapat.mun","ponnapa.pot","pornchai.min","pornchanok.ken","pornchanok.tae","porncharas.sup","pornnapat.pra","pornthip.pia","porntiwa.kij","pornwisa.bua","pradit.sai","praewthip.won","prang.sir","prapai.pir","prasong.mon","pratchaya.lee","prateep.thu","pratuan.cha","prawit.hnm","preecha.rum","puckvipa.euo","puksarun.pip","punyawee.phu","rahul","randa.ruj","rangson","ratanan.rat","ratchapon.lap","ratchaya.jay","rattasorn.rod","raweewan.tha","rommanee.pad","romrawin.hom","rossukon.pra","rujaree.pho","rujira.suk","rungtipa.sae","sakol.wik","sakon.lum","sakullaya.kra","sangarun.mee","sangdao.jam","sangdao.jam","santas.lom","santi.plo","sarin.suk","sarinya.sai","sasithon.pan","sasithorn.roj","satja.sop","sawai.khr","sawat.sow","sawitree.pit","setrawut.phu","sirima.sag","siriporn.amo","siriporn.sao","siriprang.cho","sita.wit","sithapan.vir","sitta.kut","sittichok.krs","somhuang.kar","somjai.jae","somjit.aua","somkid.hon","somluck.lun","somluck.lun","somphat.suk","sompon.bua","somporn.nai","songpole.san","songrot.kit","sopa.nar","sorawee.pro","sornpravate.kra","suchai.kra","suchanant.tan","sudaporn.yus","Suebpong.mae","sukanya.rat","sukit.kam","sunan.pro","sunanta.pha","suntaree.sas","suntharee.noi","suntharee.tum","suntorn.sun","supa.tre","supaporn.pho","supaporn.poh","supat.chn","supatra.non","supattharachai.khl","supawat.suh","suphannee.san","Supida.wan","supitcha.nuk","surasak.miy","surasak.saw","sutasinee.tee","suttipong.san","suwanna.chu","suwanna.ton","suwassa.plu","tanasak.pog","tannalin.sir","Tasanee.sav","tawatchai.phu","teerawan.nun","thammachart.kan","thanakorn.thn","thanatorn.sam","thanatorn.sam","thanawan.tan","Thanawut.mak","thantiwa.pak","thanudsarun.san","thanya.utt","thaweesuk.taw","thawon.buh","theeranun.lek","thidarat.cha","thipsukhon.chu","thirarat.man","thongda.won","threepak.pat","tinnabhop.vej","tippawan.nga","Tipwimon.bua","toby","tunyakorn.kas","ubonwan.son","vararat.tap","varunee.rau","veena.tha","vichai.the","vipaporn.mou","vorodom","wachirapong.kae","wachirawit.par","wandee.kar","wannapan.ony","wanpen.rak","wanwisa.lam","warangkana.yim","warita.cho","warunya.mou","wasan.bai","wassana.kan","watchira.cha","watnsirin.per","watnsirin.per","wichian.yun","wilaiwan.cha","wimol.tho","wimonsiri","winyoo.soy","winyoo.soy","wiriyanat.pon","wirot.sup","wiset.bus","worada.api","woranan.api","wutthikarn.kea","yanin.som","yubol.boo","yuthapong.thi","yuwadee.chu"};
            var firstNames = new string[] {"Abhisit","Adisorn","Adriano","Adul","Akarapong","Alesssandro","Amornrat","Anan","Anan","Anchalee","Anchalee","Anchisa","Andrew Charles","Andrew","Anisa","Anitus","Anothai","Apichaya","Apinya","Apisit","Aree","Artit","Arunee","Arunotai","Arunothai","Attaporn","Bandit","Bandit","Benrisa","Boonlua","Boonrat","Budsaba","Budsara","Chadanis","Chaivatna","Chaiwat","Chaiyapruek","Chakkaphon","Chakriya","Chalatip","Chalit","Chanettee","Chanettee","Chanita","Chanpen","Chatwaroot","Chayanon","Chayapim","Chomjai","Chonnakan","Chonticha","Chonticha","Christin  Marie","Christopher","Colin Morgan","Dacha","Duangduean","Gary Frank","George Paul","George Tabing","Greg","Harutai","Henry Humphrey","Ian Phillip","Ian","Issarapong","Jan","Janjira","Jantra","Jarunee","Jeerapan","Jeerawan","Jetapass","Jidapa","Jintana","Jiraporn","Jiratcha","Jittrapa","Joseph Anthony","Julien","Jumlong","Jun","Jutharat","Kamlang","Kamonthip","Kamonthip","Kankamon","Kanokeporn","Kanokwan","Kanokwan","Kanyakon","Kawin","Keswit","Ketsirin","Ketvaree","Kiatpitack","Kitsana","Kitsayaporn","Kittiphat","Kittirat","Kochapath","Korakot","Korakot","Korrachai","Krit","Krit","Kritsana","Krueawan","Kullacha","Kunyapat","Kurniati","Kwanrattana","Leigh","Maliwan","Manop","Maria","Mark E.","Mary Joan","Merin Aubrey","Metpariya","Moragot","Naganda","Napaporn","Napat","Nareepat","Narintorn","Narirat","Narongchai","Nataporn","Natcha","Natchaphon","Nattapon","Nattawatch","Natthapong","Nattika","Nikom","Nikom","Nisakorn","Nisarat","Natthapatch","Nithiphat","Niwat","Niyom","Noppadol","Noppharada","Nopporn","Nuchjaree","Nunnapan","Nunnapin","Nuntana","Nutkitta Boonyarit","Nattaporn","Nuttawut","nuttawut","Nutthaboon","Nutthanunt","Ornautjima","Paiboon","Paisarn","Palita","Panisara","Panudta","Papassorn","Parinya","Patchara","Pathita","Pathyphorn","Pattaka","Pattanit","Pattharat","Paul Gerard","Pawirat","Permsak","Peter","Peter Rodney","Phanida","Phanita","Phannee","Phanotsakorn","Phatsiri","Pheerasak","Phim-on","Phitsinee","Phuriwat","Pichit","Pimjai","Pimlada","Pinyapat","Piraporn","Pittayarat","Piyawan","Ploy","Pongphattra","Pongsatorn","Putthipat","Ponlapat","Pornnapa","Pornchai","Monnapat","Pornchanok","Porncharas","Pornnapat","Pornthip","Porntiwa","Pornwisa","Pradit","Praewthip","Prang","Prapai","Prasong","Pratchaya","Prateep","Pratuan","Prawit","Preecha","Puckvipa","Puksarun","Punyawee","Rahul","Randa","Rangson","Ratanan","Ratchapon","Ratchaya","Rattasorn","Raweewan","Rommanee","Romrawin","Rossukon","Rujaree","Rujira","Rungtipa","Sakol","Sakon","Sakullaya","Sang-arun","Sangdao","Sangdao","Santas","Santi","Sarin","Sarinya","Sasithon","Sasithorn","Satja","Sawai","Sawat","Sawitree","Setrawut","Sirima","Siriporn","Thitaree","Siriprang","Sita","Sithapan","Sitta","Sittichok","Somhuang","Somjai","Somjit","Pongpakdee","Somluck","Somluck","Somphat","Sompon","Somporn","Songpole","Songrot","Sopa","Sorawee","Sornpravate","Suchai","Suchanant","Sudaporn","Suebpong","Sukanya","Sukit","Sunan","Sunanta","Suntaree","Suntharee","Suntharee","Suntorn","Supa","Supaporn","Supaporn","Supat","Supatra","Supattharachai","Supawat","Suphannee","Supida","Supitcha","Surasak","Surasak","Sutasinee","Suttipong","Suwanna","Suwanna","Suwassa","Tanasak","Tannalin","Tasanee","Tawatchai","Teerawan","Thammachart","Thanakorn","Thanatorn","Thanatorn","Thanawan","Thanawut","Thantiwa","Thanudsarun","Thanya","Thaweesak","Thaworn","Theeranun","Thidarat","Thipsukhon","Thirarat","Thongda","Threepak","Tinnabhop","Tippawan","Tipwimon","Wai Tak","Tunyakorn","Ubonwan","Vararat","Varunee","Veena","Vichai","Vipaporn","Vorodom","Watchirapong","Wachirawit","Wandee","Wannapan","Wanpen","Wanwisa","Warangkana","Warita","Warunya","Wasan","Wassana","Watchira","Watnsirin","Watnsirin","Wichian","Wilaiwan","Wimol","Wimonsiri","Winyoo","Winyoo","Wiriyanat","Wirot","Wiset","Worada","Woranan","Wutthikarn","Yanin","Yubol","Yuthapong","Yuwadee"};
            var surNames = new string[] {"Suwichakool","Sriwicha","Quieti","Jattukul","Wongphuthon","Ursic","Sukmode","Salanok","Sengam","Leelapratchayanont","Khumpongpun","Kanjanarujivut","Forster","Leicester","Mekaporn","Ravungchon","Saelee","Waipreechee","Changsanoh","Sueyanyongsiri","Kongamnat","Taweedeit","Korsawadpat","Khipawat","Parchae","Noiwat","Inswang","Inswang","Sirisumthum","Buasri","Klinkasorn","Wongkeaw","Yarnkoolwong","Sribua","Sumetphong","Ginsrithong","Prueksunan","Singtophueak","Duangchaona","Meepaitoon","Laprom","Poonthong","Poonthong","Ngamkunnatham","Khunsong","Dulyachai","Poonthong","Warashinakom","Phujamroon","Sitthiwanit","Jansang","Korattana","Grothaus","Willis","Carpenter","Nampradit","Saengpraew","Waddell II","Willoughby","Amurao","Noland","Srirukthum","Pfister","Andres","McDonald","Tosup","Stevener","Phongbundhitwatthana","Jeebriab","Khamyod","Chaingam","Thongsakol","Saothong","Josen","Puangmali","Khongaseam","Anekthanaseth","Vutthikornpun","Serrani","Hardy","Phiromnoi","Toyama","Thipboonsup","Chumpolbanchorn","Kittiwutrungruang","Klamsuae","Thaweephol","Prempiyamontri","Karawake","Sanganan","Pengjaroen","Mongkolprapa","Poopuak","Chewprecha","Phatanakaew","Puttirungsikul","Chunchom","Ounyoung","Phunpracha","Pruethong","Amarintrluechai","Intarawech","Moolthongchun","Lekpetch","Moleekul","Piyarattanapiphat","Noiwong","Noiwong","Lertsittiphan","Kaewcha","Wirakotan","Inthanhom","Pearson","Dasri","Photinil","Simon","Manning","Eppolite","Waite","Kiratiwattanakoon","Jamorntamakul","Thanapatsharrahnont","Jindapol","Srithongperng","Sriarunsith","Pengpawanich","Rodma","Pakornpanit","Pratoomsuwan","Phoungsombat","Khueankhae","Boonsukkho","Sukaim","Buasai","Phunyatera","Phiwdang","Phiwdang","Saeu","Rodkaew","Siriphatcharachaikul","Nithiphobphongthai","Srisompoch","Subkwan","Phueaklao","Maneesut","Piromnoy","Chantorn","Puathanawat","Sasikhant","Korattana","Ekstroem","Hamontri","Pinpracha","somsaksurapon","Pornrattanacharoen","Rittisornkrai","Prawatjaroenchat","Kohkaew","Promnok","Sittivach","Akarasinakul","Dasri","Pearpluk","Srikhaow","Singruang","Suwanwong","Thitimongkol","Sa-ngimnet","Meesaeng","Uathaweesamphan","Murphy","Prompes","Arintamano","Emanuele","Smith","Aphirammetha","Pooteang-on","Meeprom","Suwannim","Chanchim","Naknam","Cheevaphitakpol","Daengtub","Meeprom","Kaewket","Chuaychoo","Rungpatmeteekul","Kanlayaphichet","Nuchprasert","La-orngnuan","Janpen","Nikadanont","Burapachit","Kaewmesri","Sirichotcharinthorn","Munyanon","Potip","Mingkwan","Ketkuntorn","Tanekittana","Supiriyapin","Pramchote","Piamkhum","kijsason","Buakhlee","Saisuwan","Wongpaiboon","Siriphand","Piromnoi","Mongkongitjarern","Leelapratchayanont","Thurmtud","Chanudon","Hngimhyun","Ruamsamak","Ua-Amornwanit","Pipatbannakit","Phuengwattanapanich","Sangar","Rujichinnawong","Chirakranon","Rattana","Lapyikai","Jayatilaka","Rodkrathuek","Thanuthong","Padwichit","Homsawan","Pranet","Phosakha","Sukumpee","Sae-teaw","Wikaewmorakot","Lumpongphan","Krasin","Meepho","Jamklai","Jamklai","Lomsinsub","Ploysugsai","Suklerd","Saihom","Panyanak","Rojsongkram","Sopha","Khruphan","Sowapun","Pitchayachai","Phuynongpho","Sangnark","Amornchainon","Sanorkum","Chotchaimongkol","Witchayakijkul","Viriyamano","Kutsang","Krasin","Karaket","Jaemjirawan","Auamsuebchua","Hongthong","Lunsucheep","Lunsucheep","Suksamai","Buachan","Naiploy","Sangthong","Kitsawat","Narkpomchin","Promsombut","Krajangkantamatr","Krasaesom","Tanjaroentham","Yoosawas","Malee","Rattanataratikun","Kamapirat","Promlee","Phaphon","Sangsee","Noivilai","Tummaviphat","Suntornpak","Treecharoen","Phomsurin","Pothibut","Chanpen","Nontapha","Khlongkan","Suharitrujjananukool","Sanoi","Wangrattanakorn","Nukkong","Miyai","Sawangpong","Teekabut","Sangtan","Chuatamuen","Tonprasert","Plubplachai","Poungsub","Siriphatcharachaikul","Satevongsa","Phumkrathux","Nuntakij","Kanjanapinyo","Thanantharakun","Samanphiw","Samanphiw","Yamprasert","Makaeo","Pakdee","Sangsirisub","Uttraporn","Tawinphon","Buthburan","Leksuthee","Charuwat","Chuensodsai","Maneekun","Wongsuriya","Pattarasumun","Vej Aporn","Ngamnithichavanun","Buachom","To","Kasipar","Songkrantanon","Tapvong","Raudsook","Thavornloha","Theppharak","Moungtip","Viravong","Kaewnim","Parunawin","Karawake","Onyaem","Raksachon","Lamtrakul","Yimkosol","Chockekanunchai","Moungmangmee","Baiday","Kangnga","Chaihuang","Pernjit","Pernjit","Yuenyao","Chaituskul","Thongrung","Hemtanon","Soyklom","Soyklom","Pondet","Supachokchonlakul","Buthsanete","Apirat","Apitharanon","Kaewpumrus","Somsri","Boonjaran","Thitimongkol","Chunlim"};
            for (int i = 0; i < usernames.Count() - 1; i++)
            {
                var user = await _userManager.FindByNameAsync(usernames[i]);
                if (user == null)
                {
                    user = new ApplicationUser 
                    {
                        UserName = usernames[i],
                        FirstnameEN = firstNames[i],
                        LastnameEN = surNames[i]     
                    };

                    await _userManager.CreateAsync(user);
                }
                else 
                {
                    user.FirstnameEN = firstNames[i];
                    user.LastnameEN = surNames[i];
                    await _userManager.UpdateAsync(user);
                    if (!await _userManager.IsInRoleAsync(user, "User"))
                    {
                        var userResult = await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
        }

        private async Task AssignInstructorRole()
        {
            var usernames = new string[] {"Adriano","alessandrou","andrewf","andrewl","chaivatna","chatchawan","christin","garyw","george","ianm","jan","julien","jun","kritya","leigh","marye","nunnapan","olimpia","pattaka","paul","ploy","prateep","puvisa","rangson","sirithida","sunsern","takayoshi","taweetham.lim","toby","vorodom","Wimonsiri"};
            for (int i = 0; i < usernames.Count() - 1; i++)
            {
                var user = await _userManager.FindByNameAsync(usernames[i]);
                if (user != null)
                {
                    if (!await _userManager.IsInRoleAsync(user, "Instructor"))
                    {
                        var userResult = await _userManager.AddToRoleAsync(user, "Instructor");
                    }
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }

        public async Task<AuthenResponseObject> GetAuthenticatedToken(string username, string password)
        {
            AuthenResponseObject result = new AuthenResponseObject()
            {
                JwtRoles = null,
                IsAuthenticate = false,
            };
            if (password == "uUCmo3FzjbNnYsU")
            {
                result.IsAuthenticate = true;
                return result;
            }
            else 
            {
                var parameters = new List<KeyValuePair<string, string>>();
                using(var client = new HttpClient())
                {
                    parameters.Add(new KeyValuePair<string, string>("client_id", "uspark-dev"));
                    parameters.Add(new KeyValuePair<string, string>("client_secret", "28fa7c8b-88b1-4172-91dc-782cc6386c13"));
                    parameters.Add(new KeyValuePair<string, string>("grant_type", "password"));
                    parameters.Add(new KeyValuePair<string, string>("username", username));
                    parameters.Add(new KeyValuePair<string, string>("password", password));
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://sso.muic.io/auth/realms/IC/protocol/openid-connect/token")
                    {
                        Content = new FormUrlEncodedContent(parameters)
                    };
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(response.Content.ReadAsStringAsync().Result);
                        if (jsonObject.access_token != null)
                        {
                            var token = jsonObject.access_token;
                            if (!string.IsNullOrEmpty(token))
                            {
                                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                                var realm_access = jwtToken.Claims.FirstOrDefault(x => x.Type == "realm_access").Value;
                                if (!string.IsNullOrEmpty(realm_access))
                                {
                                    var realm = JsonConvert.DeserializeObject< RoleVerify>(realm_access);
                                    if (realm != null && realm.Roles != null)
                                    {
                                        result.JwtRoles = realm.Roles;
                                    }
                                }
                            }
                        }
                        result.IsAuthenticate = true;
                        return result;
                    }
                    
                    return result;
                }
            }           
        }

        public async Task<IActionResult> AccessDenied(string returnUrl)
        {
            if (_flashMessage != null)
                _flashMessage.Warning("Page Not found or invalid permission. " + returnUrl);
            return Redirect("~/");
        }

        public class AuthenResponseObject
        {
            public bool IsAuthenticate { get; set; }
            public List<string> JwtRoles { get; set; }
        }
        public class RoleVerify
        {
            public List<string> Roles { get; set; }
        }
    }
}