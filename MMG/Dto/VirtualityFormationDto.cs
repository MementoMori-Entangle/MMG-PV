using System;
using System.Collections.Generic;

namespace MMG.Dto
{
    [Serializable]
    public class VirtualityFormationDto : Dto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public VirtualityFormationDto() { }

        public Dictionary<int, string> GetKeyValuePairs()
        {
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();

            List<VirtualityFormationDto> virtualityFormationDtos = GetDefaultList();

            foreach (VirtualityFormationDto virtualityFormationDto in virtualityFormationDtos)
            {
                keyValuePairs.Add(virtualityFormationDto.Id, virtualityFormationDto.Name);
            }

            return keyValuePairs;
        }

        public List<VirtualityFormationDto> GetDefaultList()
        {
            List<VirtualityFormationDto> virtualityFormationDtos = new List<VirtualityFormationDto>
            {
                new VirtualityFormationDto()
                {
                    Id = 0,
                    Name = "未使用"
                },
                new VirtualityFormationDto()
                {
                    Id = 1,
                    Name = "コル、フロ+デバフ40"
                },
                new VirtualityFormationDto()
                {
                    Id = 2,
                    Name = "両軸+デバフ45"
                },
                new VirtualityFormationDto()
                {
                    Id = 3,
                    Name = "コル、フロ、ニーナ+デバフ35"
                },
                new VirtualityFormationDto()
                {
                    Id = 4,
                    Name = "コル、フロ、ニーナ+サブパ3+デバフ30"
                },
                new VirtualityFormationDto()
                {
                    Id = 5,
                    Name = "デバフ要員(35キャラ)"
                },
                new VirtualityFormationDto()
                {
                    Id = 6,
                    Name = "はいせさん級(全キャラ保持最強)"
                },
                new VirtualityFormationDto()
                {
                    Id = 7,
                    Name = "コル、フロ+サブパ1+デバフ30"
                },
                new VirtualityFormationDto()
                {
                    Id = 8,
                    Name = "エルフ主軸、フロ+デバフ45"
                },
                new VirtualityFormationDto()
                {
                    Id = 9,
                    Name = "両軸、ニーナ+サブパ3+デバフ30"
                },
                new VirtualityFormationDto()
                {
                    Id = 10,
                    Name = "ニーナ+デバフ50"
                },
                new VirtualityFormationDto()
                {
                    Id = 11,
                    Name = "コル、フロ、ルサールカ+デバフ40"
                },
                new VirtualityFormationDto()
                {
                    Id = 12,
                    Name = "フロパ、ニーナパ+デバフ40"
                },
                new VirtualityFormationDto()
                {
                    Id = 13,
                    Name = "コルパ、ニーナパ+デバフ40"
                }
            };

            return virtualityFormationDtos;
        }
    }
}
