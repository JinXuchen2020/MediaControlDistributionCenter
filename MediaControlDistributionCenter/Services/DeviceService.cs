using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SqlSugar;

namespace MediaControlDistributionCenter.Services
{
    public class DeviceService : IDeviceService
    {
        public async Task<IEnumerable<DeviceViewModel>> GetDevices(int userId, int? groupId = null)
        {
            var results = SQLite.QueryTable<Device>()
                .InnerJoin<User>((d, u) => d.UserId == u.Id)
                .LeftJoin<DeviceGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserId == dg.UserId)
                .Where((d, u, dg) => (groupId == null ? true : d.GroupId == groupId) && d.UserId == userId)
                .GroupBy(d => d.Id)
                .Select((d, u, dg) => new
                {
                    Device = d,
                    User = u,
                    Group = dg,
                    Medias = SqlFunc.Subqueryable<DeviceMedia>().InnerJoin<Media>((dm, m) => dm.MediaId == m.Id).Where((dm, m) => dm.DeviceId == d.Id).ToList((dm, m) => m)
                })
                .ToList();

            return await Task.FromResult(results.Select(c =>
            {
                c.Device.Medias = c.Medias;
                c.Device.User = c.User;
                c.Device.Group = c.Group;
                return new DeviceViewModel(c.Device);
            }));
        }

        public async Task<IEnumerable<DeviceViewModel>> GetDevices()
        {
            var results = SQLite.QueryTable<Device>()
                .InnerJoin<User>((d, u) => d.UserId == u.Id)
                .LeftJoin<DeviceGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserId == dg.UserId)
                .GroupBy(d => d.Id)
                .Select((d, u, dg) => new
                {
                    Device = d,
                    User = u,
                    Group = dg,
                    Medias = SqlFunc.Subqueryable<DeviceMedia>().InnerJoin<Media>((dm, m) => dm.MediaId == m.Id).Where((dm, m) => dm.DeviceId == d.Id).ToList((dm, m) => m)
                })
                .ToList();

            return await Task.FromResult(results.Select(c => 
            {
                c.Device.Medias = c.Medias;
                c.Device.User = c.User;
                c.Device.Group = c.Group;
                return new DeviceViewModel(c.Device);
            }));
        }

        public async Task<IEnumerable<DeviceViewModel>> GetAgentDevices(int agentId)
        {
            var results = SQLite.QueryTable<Device>()
                .InnerJoin<User>((d, u) => d.UserId == u.Id)
                .LeftJoin<DeviceGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserId == dg.UserId)
                .Where((d, u, dg) => u.AgentId == agentId)
                .GroupBy(d => d.Id)
                .Select((d, u, dg) => new
                {
                    Device = d,
                    User = u,
                    Group = dg,
                    Medias = SqlFunc.Subqueryable<DeviceMedia>().InnerJoin<Media>((dm, m) => dm.MediaId == m.Id).Where((dm, m) => dm.DeviceId == d.Id).ToList((dm, m) => m)
                })
                .ToList();

            return await Task.FromResult(results.Select(c =>
            {
                c.Device.Medias = c.Medias;
                c.Device.User = c.User;
                c.Device.Group = c.Group;
                return new DeviceViewModel(c.Device);
            }));
        }

        public async Task<IEnumerable<MediaViewModel>> GetMedias(int userId, int? groupId = null)
        {
            var results = SQLite.QueryTable<Media>()
                .InnerJoin<User>((d, u) => d.UserId == u.Id)
                .LeftJoin<MediaGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserId == dg.UserId)
                .Where((d, u, dg) => (groupId == null ? true : d.GroupId == groupId) && d.UserId == userId)
                .GroupBy(d => d.Id)
                .Select((d, u, dg) => new
                {
                    Media = d,
                    User = u,
                    Group = dg,
                    Devices = SqlFunc.Subqueryable<DeviceMedia>().InnerJoin<Device>((dm, m) => dm.DeviceId == m.Id).Where((dm, m) => dm.MediaId == d.Id).ToList((dm, m) => m)
                })
                .ToList();

            return await Task.FromResult(results.Select(c =>
            {
                c.Media.Devices = c.Devices;
                c.Media.User = c.User;
                c.Media.Group = c.Group;
                return new MediaViewModel(c.Media);
            }));
        }

        public async Task<IEnumerable<DeviceTimeControlViewModel>> GetDeviceTimeControls(int deviceId, string type)
        {
            var results = SQLite.QueryTable<DeviceControl>()
                .Where(c => c.DeviceId == deviceId && c.Type == type)
                .ToList();

            return await Task.FromResult(results.Select(c => new DeviceTimeControlViewModel(c)));
        }

        public async Task<IEnumerable<DeviceGroupViewModel>> GetDeviceGroups(int? userId = null)
        {
            if (userId != null)
            {
                var results = SQLite.QueryTable<DeviceGroup>().Where(d => d.UserId == userId).ToList();
                return await Task.FromResult(results.Select(c => new DeviceGroupViewModel(c)));
            }
            else
            {
                var results = SQLite.QueryTable<DeviceGroup>().ToList();
                return await Task.FromResult(results.Select(c => new DeviceGroupViewModel(c)));
            }
        }

        public async Task<IEnumerable<MediaGroupViewModel>> GetMediaGroups(int? userId = null)
        {
            if (userId != null)
            {
                var results = SQLite.QueryTable<MediaGroup>().Where(d => d.UserId == userId).ToList();
                return await Task.FromResult(results.Select(c => new MediaGroupViewModel(c)));
            }
            else
            {
                var results = SQLite.QueryTable<MediaGroup>().ToList();
                return await Task.FromResult(results.Select(c => new MediaGroupViewModel(c)));
            }
        }
    }
}
